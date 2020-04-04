using System;
using System.Threading.Tasks;
using Autofac;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Job.QuorumTransactionWatcher.Contract;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Sdk;

namespace MAVN.Job.QuorumTransactionWatcher.Services
{
    public class StartupManager : IStartupManager
    {
        private readonly IRabbitPublisher<CustomerRegisteredInBlockchainEvent> _customerRegisteredEventPublishingService;
        private readonly IndexingService _indexingService;
        private readonly BlocksLagWatcher _blocksLagWatcher;
        private readonly ILog _log;
        private readonly IRabbitPublisher<TransactionFailedInBlockchainEvent> _transactionFailedEventPublishingService;
        private readonly IRabbitPublisher<MintEvent> _mintEventPublishingService;
        private readonly IRabbitPublisher<TransferEvent> _transferEventPublishingService;
        private readonly IRabbitPublisher<UndecodedEvent> _undecodedEventPublishingService;
        private readonly IRabbitPublisher<TransactionCompletedInBlockchainEvent> _trasactionCompletedEventPublishingService;
        private readonly IRabbitPublisher<StakeIncreasedEvent> _stakeIncreasedEventPublishingService;
        private readonly IRabbitPublisher<StakeReleasedEvent> _stakeReleasedEventPublishingService;
        private readonly IRabbitPublisher<FeeCollectedEvent> _feeCollectedEventPublishingService;
        private readonly IRabbitPublisher<SeizedFromEvent> _seizedFromEventPublishingService;

        public StartupManager(
            IRabbitPublisher<CustomerRegisteredInBlockchainEvent> customerRegisteredEventPublishingService,
            IndexingService indexingService,
            BlocksLagWatcher blocksLagWatcher,
            ILogFactory logFactory,
            IRabbitPublisher<TransactionFailedInBlockchainEvent> transactionFailedEventPublishingService,
            IRabbitPublisher<MintEvent> mintEventPublishingService,
            IRabbitPublisher<TransferEvent> transferEventPublishingService,
            IRabbitPublisher<UndecodedEvent> undecodedEventPublishingService,
            IRabbitPublisher<TransactionCompletedInBlockchainEvent> trasactionCompletedEventPublishingService,
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
            _trasactionCompletedEventPublishingService = trasactionCompletedEventPublishingService;
            _stakeIncreasedEventPublishingService = stakeIncreasedEventPublishingService;
            _stakeReleasedEventPublishingService = stakeReleasedEventPublishingService;
            _feeCollectedEventPublishingService = feeCollectedEventPublishingService;
            _blocksLagWatcher = blocksLagWatcher;
            _seizedFromEventPublishingService = seizedFromEventPublishingService;
        }

        public async Task StartAsync()
        {
            StartService(_undecodedEventPublishingService,"Undecoded event publishing service");
            StartService(_trasactionCompletedEventPublishingService, "Transaction completed event publishing service");
            StartService(_mintEventPublishingService, "Mint event publishing service");
            StartService(_customerRegisteredEventPublishingService, "Customer registered event publishing service");
            StartService(_transactionFailedEventPublishingService, "Transaction failed event publishing service");
            StartService(_transferEventPublishingService, "Transfer event publishing service");
            StartService(_stakeIncreasedEventPublishingService, "Stake increased event publishing service");
            StartService(_stakeReleasedEventPublishingService, "Stake released event publishing service");
            StartService(_feeCollectedEventPublishingService, "Fee Collected event publishing service");
            StartService(_seizedFromEventPublishingService, "Seized From event publishing service");
            StartService(_indexingService, "Indexing service");
            StartService(_blocksLagWatcher, "Blocks lag watcher");

            await Task.CompletedTask;
        }

        private void StartService(
            IStartable service,
            string serviceName)
        {
            try
            {
                #region Logging
                
                _log.Info($"{serviceName} is starting.");
                
                #endregion

                service.Start();
                
                #region Logging
                
                _log.Info($"{serviceName} has been started.");
                
                #endregion
            }
            catch (Exception e)
            {
                #region Logging
                
                _log.Critical(e, $"{serviceName} starting failed.");
                
                #endregion
                
                throw;
            }
        }
    }
}
