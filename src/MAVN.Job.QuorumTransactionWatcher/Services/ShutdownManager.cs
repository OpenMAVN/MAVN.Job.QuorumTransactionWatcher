using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Job.QuorumTransactionWatcher.Contract;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Sdk;

namespace MAVN.Job.QuorumTransactionWatcher.Services
{
    public class ShutdownManager : IShutdownManager
    {
        private readonly IRabbitPublisher<CustomerRegisteredInBlockchainEvent> _customerRegisteredEventPublishingService;
        private readonly IndexingService _indexingService;
        private readonly BlocksLagWatcher _blocksLagWatcher;
        private readonly ILog _log;
        private readonly IRabbitPublisher<TransactionFailedInBlockchainEvent> _transactionFailedEventPublishingService;
        private readonly IRabbitPublisher<MintEvent> _mintEventPublishingService;
        private readonly IRabbitPublisher<TransferEvent> _transferEventPublishingService;
        private readonly IRabbitPublisher<UndecodedEvent> _undecodedEventPublishingService;
        private readonly IRabbitPublisher<TransactionCompletedInBlockchainEvent> _transactionCompletedEventPublishingService;
        private readonly IRabbitPublisher<StakeIncreasedEvent> _stakeIncreasedEventPublishingService;
        private readonly IRabbitPublisher<StakeReleasedEvent> _stakeReleasedEventPublishingService;
        private readonly IRabbitPublisher<FeeCollectedEvent> _feeCollectedEventPublishingService;
        private readonly IRabbitPublisher<SeizedFromEvent> _seizedFromEventPublishingService;

        public ShutdownManager(
            IRabbitPublisher<CustomerRegisteredInBlockchainEvent> customerRegisteredEventPublishingService,
            IndexingService indexingService,
            BlocksLagWatcher blocksLagWatcher,
            ILogFactory logFactory,
            IRabbitPublisher<TransactionFailedInBlockchainEvent> transactionFailedEventPublishingService,
            IRabbitPublisher<MintEvent> mintEventPublishingService,
            IRabbitPublisher<TransferEvent> transferEventPublishingService,
            IRabbitPublisher<UndecodedEvent> undecodedEventPublishingService,
            IRabbitPublisher<TransactionCompletedInBlockchainEvent> transactionCompletedEventPublishingService,
            IRabbitPublisher<StakeIncreasedEvent> stakeIncreasedEventPublishingService,
            IRabbitPublisher<StakeReleasedEvent> stakeReleasedEventPublishingService,
            IRabbitPublisher<FeeCollectedEvent> feeCollectedEventPublishingService,
            IRabbitPublisher<SeizedFromEvent> seizedFromEventPublishingService)
            
        {
            _customerRegisteredEventPublishingService = customerRegisteredEventPublishingService;
            _indexingService = indexingService;
            _log = logFactory.CreateLog(this);
            _transactionFailedEventPublishingService = transactionFailedEventPublishingService;
            _mintEventPublishingService = mintEventPublishingService;
            _transferEventPublishingService = transferEventPublishingService;
            _undecodedEventPublishingService = undecodedEventPublishingService;
            _transactionCompletedEventPublishingService = transactionCompletedEventPublishingService;
            _stakeIncreasedEventPublishingService = stakeIncreasedEventPublishingService;
            _stakeReleasedEventPublishingService = stakeReleasedEventPublishingService;
            _feeCollectedEventPublishingService = feeCollectedEventPublishingService;
            _blocksLagWatcher = blocksLagWatcher;
            _seizedFromEventPublishingService = seizedFromEventPublishingService;
        }

        public async Task StopAsync()
        {
            await StopServiceAsync(_indexingService, "Indexing service");
            await StopServiceAsync(_stakeIncreasedEventPublishingService, "Stake increased event publishing service");
            await StopServiceAsync(_stakeReleasedEventPublishingService, "Stake released event publishing service");
            await StopServiceAsync(_customerRegisteredEventPublishingService, "Customer registered event publishing service");
            await StopServiceAsync(_transactionFailedEventPublishingService, "Transaction failed event publishing service");
            await StopServiceAsync(_mintEventPublishingService, "Mint event publishing service");
            await StopServiceAsync(_transferEventPublishingService, "Transfer event publishing service");
            await StopServiceAsync(_transactionCompletedEventPublishingService, "Transaction completed event publishing service");
            await StopServiceAsync(_undecodedEventPublishingService, "Undecoded event publishing service");
            await StopServiceAsync(_feeCollectedEventPublishingService, "Fee collected event publishing service");
            await StopServiceAsync(_blocksLagWatcher, "Blocks lag watcher");
            await StopServiceAsync(_seizedFromEventPublishingService, "Seized From event publishing service");
        }

        private Task StopServiceAsync(
            IStopable service,
            string serviceName)
        {
            try
            {
                #region Logging
            
                _log.Info($"{serviceName} is shutting down.");
            
                #endregion
            
                service.Stop();
            
                #region Logging
            
                _log.Info($"{serviceName} has been shutdown.");
            
                #endregion
            }
            catch (Exception)
            {
                #region Logging
                
                _log.Warning($"{serviceName} shutdown failed.");
                
                #endregion
            }

            return Task.CompletedTask;
        }
    }
}
