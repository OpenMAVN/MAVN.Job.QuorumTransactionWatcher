using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Job.QuorumTransactionWatcher.Contract;
using MAVN.Job.QuorumTransactionWatcher.Domain.Repositories;
using MAVN.Job.QuorumTransactionWatcher.Domain.Services;
using MAVN.Numerics;
using MAVN.Job.QuorumTransactionWatcher.Domain;
using MAVN.PrivateBlockchain.Definitions;
using Lykke.RabbitMqBroker.Publisher;
using Nethereum.Contracts;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Polly;
using Polly.Retry;

namespace MAVN.Job.QuorumTransactionWatcher.DomainServices
{
    public class BlockchainIndexingService : IBlockchainIndexingService
    {
        private readonly IRabbitPublisher<CustomerRegisteredInBlockchainEvent> _customerRegisteredInBlockchainEventPublishingService;
        private readonly IRabbitPublisher<MintEvent> _mintEventPublishingService;
        private readonly IRabbitPublisher<TransferEvent> _transferEventPublishingService;
        private readonly IIndexingStateRepository _indexingStateRepository;
        private readonly ILog _log;
        private readonly IRabbitPublisher<TransactionFailedInBlockchainEvent> _transactionFailedInBlockchainEventPublishingService;
        private readonly IRabbitPublisher<TransactionCompletedInBlockchainEvent> _transactionCompletedInBlockchainEventPublishingService;
        private readonly IRabbitPublisher<UndecodedEvent> _undecodedEventPublishingService;
        private readonly IRabbitPublisher<StakeIncreasedEvent> _stakeIncreasedPublishingService;
        private readonly IRabbitPublisher<StakeReleasedEvent> _stakeReleasedPublishingService;
        private readonly IRabbitPublisher<FeeCollectedEvent> _feeCollectedPublishingService;
        private readonly IRabbitPublisher<SeizedFromEvent> _seizedFromPublishingService;
        private readonly IEthNodeClient _ethNodeClient;
        private readonly HashSet<string> _knownAddresses;
        private readonly RetryPolicy _retryPolicy;
        
        private const int DefaultRetryCount = 10;

        private long? _lastIndexedBlock;

        public BlockchainIndexingService(
            IRabbitPublisher<CustomerRegisteredInBlockchainEvent> customerRegisteredInBlockchainEventPublishingService,
            IRabbitPublisher<MintEvent> mintEventPublishingService,
            IRabbitPublisher<TransferEvent> transferEventPublishingService,
            IIndexingStateRepository indexingStateRepository,
            ILogFactory logFactory,
            IRabbitPublisher<TransactionFailedInBlockchainEvent> transactionFailedInBlockchainEventPublishingService,
            IRabbitPublisher<TransactionCompletedInBlockchainEvent> transactionCompletedInBlockchainEventPublishingService,
            IRabbitPublisher<UndecodedEvent> undecodedEventPublishingService,
            IRabbitPublisher<StakeIncreasedEvent> stakeIncreasedPublishingService,
            IRabbitPublisher<StakeReleasedEvent> stakeReleasedPublishingService,
            IRabbitPublisher<FeeCollectedEvent> feeCollectedPublishingService,
            IRabbitPublisher<SeizedFromEvent> seizedFromPublishingService,
            IEthNodeClient ethNodeClient,
            IReadOnlyList<string> knownAddresses)
        {
            _customerRegisteredInBlockchainEventPublishingService = customerRegisteredInBlockchainEventPublishingService;
            _mintEventPublishingService = mintEventPublishingService;
            _transferEventPublishingService = transferEventPublishingService;
            _indexingStateRepository = indexingStateRepository;
            _log = logFactory.CreateLog(this);
            _transactionFailedInBlockchainEventPublishingService = transactionFailedInBlockchainEventPublishingService;
            _transactionCompletedInBlockchainEventPublishingService = transactionCompletedInBlockchainEventPublishingService;
            _undecodedEventPublishingService = undecodedEventPublishingService;
            _stakeIncreasedPublishingService = stakeIncreasedPublishingService;
            _stakeReleasedPublishingService = stakeReleasedPublishingService;
            _feeCollectedPublishingService = feeCollectedPublishingService;
            _seizedFromPublishingService = seizedFromPublishingService;
            _ethNodeClient = ethNodeClient;
            _knownAddresses = new HashSet<string>(knownAddresses.Select(o => o.ToLower()));

            _retryPolicy = Policy
                .Handle<RpcClientTimeoutException>()
                .WaitAndRetryAsync(
                    DefaultRetryCount,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (ex, _) => _log.Error(ex, "Querying transaction node with retry"));
        }

        public async Task IndexUntilLastBlockAsync()
        {
            BlocksIndexState indexState;
            do
            {
                indexState = await TryIndexNewBlocksAsync();
            } while (indexState != BlocksIndexState.UpToDate);
            _log.Info("Sleep to wait for new blocks");
        }
        
        public async Task<long> GetLastBlockFromDbAsync()
        {
            var getBestIndexedBlockNumber = _lastIndexedBlock
                                            ?? await _indexingStateRepository.GetLastIndexedBlockNumberAsync();

            return getBestIndexedBlockNumber ?? -1;
        }

        public Task<long> GetLastKnownBlockAsync()
        {
            return _retryPolicy.ExecuteAsync(_ethNodeClient.GetBlockNumberAsync);
        }

        private async Task<BlocksIndexState> TryIndexNewBlocksAsync()
        {
            var context = Guid.NewGuid().ToString();

            _log.Info("Trying to index new block", context: context);

            var lastExistingBlockNumber = await GetLastKnownBlockAsync();

            var blockNumberToIndex = await GetLastBlockFromDbAsync();

            _log.Info($"Fetched block from eth network, count of new blocks: {lastExistingBlockNumber - blockNumberToIndex}", new
            {
                context,
                lastExistingBlockNumber,
                blockNumberToIndex,
                ConuntNewBlocks = lastExistingBlockNumber - blockNumberToIndex
            });

            if (blockNumberToIndex == lastExistingBlockNumber)
                return BlocksIndexState.UpToDate;

            if (blockNumberToIndex > lastExistingBlockNumber + 10)
            {
                _log.Info($"Network last block {lastExistingBlockNumber} is lower than last indexed block {blockNumberToIndex}");
                return BlocksIndexState.Ahead;
            }

            bool isIndexed;
            var isNewBlocksExist = false;
            do
            {
                blockNumberToIndex++;
                isIndexed = await IndexBlockAsync(blockNumberToIndex, context);

                isNewBlocksExist = isNewBlocksExist || isIndexed;
            } while (isIndexed);

            return isNewBlocksExist ? BlocksIndexState.OutOfDate : BlocksIndexState.UpToDate;
        }

        private async Task<bool> IndexBlockAsync(long blockNumber, string context)
        {
            _log.Info($"Block {blockNumber} indexing started", context: context);

            var blockWithTransactions =
                await _retryPolicy.ExecuteAsync(() => _ethNodeClient.GetBlockWithTransactionHashesAsync(blockNumber));
            
            if (blockWithTransactions == null)
            {
                return false;
            }

            var transactionReceipts =
                await _retryPolicy.ExecuteAsync(() => _ethNodeClient.GetTransactionReceiptsAsync(blockWithTransactions));
            
            if (transactionReceipts.Any(receipts => receipts == null))
            {
                return false;
            }

            _log.Info($"Info about block", context: new
            {
                blockNumber,
                blockWithTransactions.Number,
                Count = blockWithTransactions.TransactionHashes?.Length,
                blockWithTransactions.Timestamp,
                blockWithTransactions.Nonce,
                blockWithTransactions.Author
            });

            foreach (var transactionReceipt in transactionReceipts)
            {
                if (transactionReceipt.Status.Value == 1)
                {
                    var undecodedEvents = ProcessUndecodedEvents(transactionReceipt);
                    var collectedFeesEvents = ProcessFeeCollectedEvents(transactionReceipt);
                    var completedTransactionEvents = ProcessCompletedTransactionAsync(transactionReceipt);
                    var processCustomerRegisteredEvents = ProcessCustomerRegisteredEvents(transactionReceipt);
                    var processMintEvents = ProcessMintEvents(transactionReceipt);
                    var processTransferEvents = ProcessTransferEvents(transactionReceipt);
                    var processStakeIncreasedEvents = ProcessStakeIncreasedEvents(transactionReceipt);
                    var processStakeDecreasedEvents = ProcessStakeDecreasedEvents(transactionReceipt);
                    var processSeizedFromEvents = ProcessSeizedEvents(transactionReceipt);

                    await Task.WhenAll(
                        completedTransactionEvents,
                        processCustomerRegisteredEvents,
                        processMintEvents,
                        processTransferEvents,
                        undecodedEvents,
                        processStakeDecreasedEvents,
                        processStakeIncreasedEvents,
                        collectedFeesEvents,
                        processSeizedFromEvents);
                }
                else
                {
                    await ProcessFailedTransactionAsync(transactionReceipt);
                }
            }

            await _indexingStateRepository.SaveLastIndexedBlockNumber(blockNumber);
            _lastIndexedBlock = blockNumber;

            #region Logging

            _log.Info(
                $"Block [{blockNumber}] has been indexed.",
                new { blockNumber, blockHash = blockWithTransactions.BlockHash, context });

            #endregion

            return true;
        }

        private async Task ProcessCustomerRegisteredEvents(TransactionReceipt transactionReceipt)
        {
            try
            {
                var events = DecodeAllEventsAndFilter<CustomerRegisteredEventDTO>(transactionReceipt);

                foreach (var @event in events)
                {
                    var evt = new CustomerRegisteredInBlockchainEvent
                    {
                        Address = @event.Event.CustomerAddress,
                        CustomerId = new Guid(@event.Event.CustomerId),
                        EventIndex = (int)@event.Log.LogIndex.Value,
                        TransactionHash = @event.Log.TransactionHash,
                        ObservedAt = DateTime.UtcNow,
                    };

                    await _customerRegisteredInBlockchainEventPublishingService.PublishAsync(evt);

                    _log.Info($"{nameof(CustomerRegisteredInBlockchainEvent)} has been published", evt);
                }
            }
            catch (Exception e)
            {
                #region Logging

                _log.Error
                (
                    e,
                    $"Failed to process customer registered events from transaction [{transactionReceipt.TransactionHash}].",
                    new { transactionHash = transactionReceipt.TransactionHash }
                );

                #endregion

                throw;
            }
        }

        private async Task ProcessMintEvents(TransactionReceipt transactionReceipt)
        {
            try
            {
                var events = DecodeAllEventsAndFilter<MintedEventDTO>(transactionReceipt);

                foreach (var @event in events)
                {
                    var evt = new MintEvent
                    {
                        TargetAddress = @event.Event.To,
                        Amount = Money18.CreateFromAtto(@event.Event.Amount),
                        EventIndex = (int)@event.Log.LogIndex.Value,
                        TransactionHash = @event.Log.TransactionHash,
                        ObservedAt = DateTime.UtcNow,
                    };

                    await _mintEventPublishingService.PublishAsync(evt);

                    _log.Info($"{nameof(MintEvent)} has been published", evt);
                }
            }
            catch (Exception e)
            {
                #region Logging

                _log.Error
                (
                    e,
                    $"Failed to process minted events from transaction [{transactionReceipt.TransactionHash}].",
                    new { transactionHash = transactionReceipt.TransactionHash }
                );

                #endregion

                throw;
            }
        }

        private async Task ProcessTransferEvents(TransactionReceipt transactionReceipt)
        {
            try
            {
                var events = DecodeAllEventsAndFilter<SentEventDTO>(transactionReceipt);

                foreach (var @event in events)
                {
                    if (string.IsNullOrEmpty(@event.Event.From) || string.IsNullOrEmpty(@event.Event.To))
                        continue;

                    var evt = new TransferEvent
                    {
                        SourceAddress = @event.Event.From,
                        TargetAddress = @event.Event.To,
                        Amount = Money18.CreateFromAtto(@event.Event.Amount),
                        EventIndex = (int)@event.Log.LogIndex.Value,
                        TransactionHash = @event.Log.TransactionHash,
                        ObservedAt = DateTime.UtcNow,
                    };

                    await _transferEventPublishingService.PublishAsync(evt);

                    _log.Info($"{nameof(TransferEvent)} has been published", evt);
                }
            }
            catch (Exception e)
            {
                #region Logging

                _log.Error
                (
                    e,
                    $"Failed to process transfer events from transaction [{transactionReceipt.TransactionHash}].",
                    new { transactionHash = transactionReceipt.TransactionHash }
                );

                #endregion

                throw;
            }
        }

        private async Task ProcessFailedTransactionAsync(TransactionReceipt transactionReceipt)
        {
            try
            {
                var evt = new TransactionFailedInBlockchainEvent
                {
                    TransactionHash = transactionReceipt.TransactionHash,
                    ObservedAt = DateTime.UtcNow,
                };

                await _transactionFailedInBlockchainEventPublishingService.PublishAsync(evt);

                _log.Info($"{nameof(TransactionFailedInBlockchainEvent)} has been published", evt);
            }
            catch (Exception e)
            {
                #region Logging

                _log.Error
                (
                    e,
                    $"Failed to process failed transaction [{transactionReceipt.TransactionHash}].",
                    new { transactionHash = transactionReceipt.TransactionHash }
                );

                #endregion

                throw;
            }
        }

        private async Task ProcessCompletedTransactionAsync(TransactionReceipt transactionReceipt)
        {
            try
            {
                var evt = new TransactionCompletedInBlockchainEvent
                {
                    TransactionHash = transactionReceipt.TransactionHash,
                    ObservedAt = DateTime.UtcNow,
                };

                await _transactionCompletedInBlockchainEventPublishingService.PublishAsync(evt);

                _log.Info($"{nameof(TransactionCompletedInBlockchainEvent)} has been published", evt);
            }
            catch (Exception e)
            {
                #region Logging

                _log.Error
                (
                    e,
                    $"Failed to process completed transaction [{transactionReceipt.TransactionHash}].",
                    new { transactionHash = transactionReceipt.TransactionHash }
                );

                #endregion

                throw;
            }
        }

        private async Task ProcessUndecodedEvents(TransactionReceipt transactionReceipt)
        {
            try
            {
                foreach (var log in transactionReceipt.Logs)
                {
                    var contractAddress = log["address"].ToObject<string>();

                    if(!IsKnownAddress(contractAddress))
                        continue;

                    var evt = new UndecodedEvent
                    {
                        TransactionHash = transactionReceipt.TransactionHash,
                        Index = log["logIndex"].ToObject<string>(),
                        OriginAddress = contractAddress,
                        Data = log["data"].ToObject<string>(),
                        Topics = log["topics"].ToObject<string[]>(),
                        ObservedAt = DateTime.UtcNow,
                        Id = Guid.NewGuid().ToString(),
                    };

                    await _undecodedEventPublishingService.PublishAsync(evt);

                    _log.Info($"{nameof(UndecodedEvent)} has been published", evt);
                }
            }
            catch (Exception e)
            {
                #region Logging

                _log.Error
                (
                    e,
                    $"Failed to process undecoded transaction [{transactionReceipt.TransactionHash}].",
                    new { transactionHash = transactionReceipt.TransactionHash }
                );

                #endregion

                throw;
            }
        }

        private async Task ProcessStakeDecreasedEvents(TransactionReceipt transactionReceipt)
        {
            try
            {
                var events = DecodeAllEventsAndFilter<StakeDecreasedEventDTO>(transactionReceipt);

                foreach (var @event in events)
                {
                    var evt = new StakeReleasedEvent
                    {
                        WalletAddress = @event.Event.Account,
                        Amount = Money18.CreateFromAtto(@event.Event.ReleasedAmount),
                        TransactionHash = transactionReceipt.TransactionHash,
                        ObservedAt = DateTime.UtcNow,
                    };

                    await _stakeReleasedPublishingService.PublishAsync(evt);

                    _log.Info($"{nameof(StakeReleasedEvent)} has been published", evt);
                }
            }
            catch (Exception e)
            {
                #region Logging

                _log.Error
                (
                    e,
                    $"Failed to process stake decreased events from transaction [{transactionReceipt.TransactionHash}].",
                    new { transactionHash = transactionReceipt.TransactionHash }
                );

                #endregion

                throw;
            }
        }

        private async Task ProcessStakeIncreasedEvents(TransactionReceipt transactionReceipt)
        {
            try
            {
                var events = DecodeAllEventsAndFilter<StakeIncreasedEventDTO>(transactionReceipt);

                foreach (var @event in events)
                {
                    var evt = new StakeIncreasedEvent
                    {
                        WalletAddress = @event.Event.Account,
                        Amount = Money18.CreateFromAtto(@event.Event.Amount),
                        TransactionHash = transactionReceipt.TransactionHash,
                        ObservedAt = DateTime.UtcNow,
                    };

                    await _stakeIncreasedPublishingService.PublishAsync(evt);

                    _log.Info($"{nameof(StakeIncreasedEvent)} has been published", evt);
                }
            }
            catch (Exception e)
            {
                #region Logging

                _log.Error
                (
                    e,
                    $"Failed to process stake increased events from transaction [{transactionReceipt.TransactionHash}].",
                    new { transactionHash = transactionReceipt.TransactionHash }
                );

                #endregion

                throw;
            }
        }

        private async Task ProcessFeeCollectedEvents(TransactionReceipt transactionReceipt)
        {
            try
            {
                var events = DecodeAllEventsAndFilter<FeeCollectedEventDTO>(transactionReceipt);

                foreach (var @event in events)
                {
                    var evt = new FeeCollectedEvent
                    {
                        Amount = Money18.CreateFromAtto(@event.Event.Amount),
                        WalletAddress = @event.Event.From,
                        Reason = @event.Event.Reason,
                        TransactionHash = transactionReceipt.TransactionHash,
                        ObservedAt = DateTime.UtcNow,
                        EventId = Guid.NewGuid().ToString()
                    };

                    await _feeCollectedPublishingService.PublishAsync(evt);

                    _log.Info($"{nameof(FeeCollectedEvent)} has been published", evt);
                }
            }
            catch (Exception e)
            {
                #region Logging

                _log.Error
                (
                    e,
                    $"Failed to process FeeCollectedEvent events from transaction [{transactionReceipt.TransactionHash}].",
                    new { transactionHash = transactionReceipt.TransactionHash }
                );

                #endregion
                throw;
            }
        }

        private async Task ProcessSeizedEvents(TransactionReceipt transactionReceipt)
        {
            try
            {
                var events = DecodeAllEventsAndFilter<SeizeFromEventDTO>(transactionReceipt);

                foreach (var @event in events)
                {
                    var evt = new SeizedFromEvent
                    {
                        Amount = Money18.CreateFromAtto(@event.Event.Amount),
                        Address = @event.Event.Account,
                        TransactionHash = transactionReceipt.TransactionHash,
                        ObservedAt = DateTime.UtcNow,
                    };

                    await _seizedFromPublishingService.PublishAsync(evt);

                    _log.Info($"{nameof(SeizedFromEvent)} has been published", evt);
                }
            }
            catch (Exception e)
            {
                #region Logging

                _log.Error
                (
                    e,
                    $"Failed to process SeizedFromEvent events from transaction [{transactionReceipt.TransactionHash}].",
                    new { transactionHash = transactionReceipt.TransactionHash }
                );

                #endregion
                throw;
            }
        }

        private IReadOnlyList<EventLog<T>> DecodeAllEventsAndFilter<T>(TransactionReceipt transactionReceipt)
            where T : new()
        {
            return transactionReceipt
                .DecodeAllEvents<T>()
                .Where(o=> IsKnownAddress(o.Log.Address))
                .ToList();
        }

        private bool IsKnownAddress(string address)
            => _knownAddresses.Contains(address.ToLower());
    }
}
