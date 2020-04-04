using Autofac;
using JetBrains.Annotations;
using Lykke.Common.Log;
using MAVN.Job.QuorumTransactionWatcher.Contract;
using MAVN.Job.QuorumTransactionWatcher.Domain.Repositories;
using MAVN.Job.QuorumTransactionWatcher.Domain.Services;
using MAVN.Job.QuorumTransactionWatcher.DomainServices;
using MAVN.Job.QuorumTransactionWatcher.Modules.Extensions;
using MAVN.Job.QuorumTransactionWatcher.Services;
using MAVN.Job.QuorumTransactionWatcher.Settings;
using MAVN.Job.QuorumTransactionWatcher.Settings.Job.Blockchain;
using MAVN.Job.QuorumTransactionWatcher.Settings.Job.Rabbit;
using Lykke.Sdk;
using Lykke.SettingsReader;

namespace MAVN.Job.QuorumTransactionWatcher.Modules
{
    [UsedImplicitly]
    public class JobModule : Module
    {
        private readonly PublisherSettings _publisherSettings;
        private readonly IReloadingManager<BlockchainSettings> _blockchainSettings;

        public JobModule(IReloadingManager<AppSettings> appSettings)
        {
            _publisherSettings = appSettings.CurrentValue.QuorumTransactionWatcherJob.Publisher;
            _blockchainSettings = appSettings.Nested(s => s.QuorumTransactionWatcherJob.Blockchain);
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterEventPublishingService<CustomerRegisteredInBlockchainEvent>(_publisherSettings);

            builder
                .RegisterEventPublishingService<TransactionFailedInBlockchainEvent>(_publisherSettings);

            builder
                .RegisterEventPublishingService<MintEvent>(_publisherSettings);

            builder
                .RegisterEventPublishingService<TransferEvent>(_publisherSettings);

            builder
                .RegisterEventPublishingService<TransactionCompletedInBlockchainEvent>(_publisherSettings);

            builder
                .RegisterEventPublishingService<UndecodedEvent>(_publisherSettings);

            builder
                .RegisterEventPublishingService<StakeIncreasedEvent>(_publisherSettings);

            builder
                .RegisterEventPublishingService<StakeReleasedEvent>(_publisherSettings);

            builder
                .RegisterEventPublishingService<FeeCollectedEvent>(_publisherSettings);

            builder
                .RegisterEventPublishingService<SeizedFromEvent>(_publisherSettings);

            var blockchainSettings = _blockchainSettings.CurrentValue;
            
            builder
                .Register(ctx => new BlockchainIndexingService
                (
                    ctx.ResolveEventPublishingService<CustomerRegisteredInBlockchainEvent>(),
                    ctx.ResolveEventPublishingService<MintEvent>(),
                    ctx.ResolveEventPublishingService<TransferEvent>(),
                    ctx.Resolve<IIndexingStateRepository>(),
                    ctx.Resolve<ILogFactory>(),
                    ctx.ResolveEventPublishingService<TransactionFailedInBlockchainEvent>(),
                    ctx.ResolveEventPublishingService<TransactionCompletedInBlockchainEvent>(),
                    ctx.ResolveEventPublishingService<UndecodedEvent>(),
                    ctx.ResolveEventPublishingService<StakeIncreasedEvent>(),
                    ctx.ResolveEventPublishingService<StakeReleasedEvent>(),
                    ctx.ResolveEventPublishingService<FeeCollectedEvent>(),
                    ctx.ResolveEventPublishingService<SeizedFromEvent>(),
                    ctx.Resolve<IEthNodeClient>(),
                    new []
                    {
                        blockchainSettings.ContractAddresses.CustomerRegistry,
                        blockchainSettings.ContractAddresses.PartnersPayments,
                        blockchainSettings.ContractAddresses.PaymentTransfers,
                        blockchainSettings.ContractAddresses.RoleRegistry,
                        blockchainSettings.ContractAddresses.Token,
                        blockchainSettings.ContractAddresses.Gateway,
                        blockchainSettings.ContractAddresses.MVNVouchersPayments
                    }
                ))
                .As<IBlockchainIndexingService>()
                .SingleInstance();

            builder
                .Register(ctx => new IndexingService
                (
                    ctx.Resolve<IBlockchainIndexingService>(),
                    ctx.Resolve<ILogFactory>(),
                    blockchainSettings.IdleTime
                ))
                .AsSelf()
                .SingleInstance();

            builder
                .Register(ctx => new EthNodeClient(
                    async () => (await _blockchainSettings.Reload()).TransactionNodeUrlList, 
                    ctx.Resolve<ILogFactory>(),
                    blockchainSettings.WebSocketsConnectionTimeOut,
                    blockchainSettings.BatchSize))
                .As<IEthNodeClient>()
                .SingleInstance();

            builder
                .Register(ctx => new StartupManager
                (
                    ctx.ResolveEventPublishingService<CustomerRegisteredInBlockchainEvent>(),
                    ctx.Resolve<IndexingService>(),
                    ctx.Resolve<BlocksLagWatcher>(),
                    ctx.Resolve<ILogFactory>(),
                    ctx.ResolveEventPublishingService<TransactionFailedInBlockchainEvent>(),
                    ctx.ResolveEventPublishingService<MintEvent>(),
                    ctx.ResolveEventPublishingService<TransferEvent>(),
                    ctx.ResolveEventPublishingService<UndecodedEvent>(),
                    ctx.ResolveEventPublishingService<TransactionCompletedInBlockchainEvent>(),
                    ctx.ResolveEventPublishingService<StakeIncreasedEvent>(),
                    ctx.ResolveEventPublishingService<StakeReleasedEvent>(),
                    ctx.ResolveEventPublishingService<FeeCollectedEvent>(),
                    ctx.ResolveEventPublishingService<SeizedFromEvent>()
                ))
                .As<IStartupManager>()
                .SingleInstance();

            builder
                .Register(ctx => new ShutdownManager
                (
                    ctx.ResolveEventPublishingService<CustomerRegisteredInBlockchainEvent>(),
                    ctx.Resolve<IndexingService>(),
                    ctx.Resolve<BlocksLagWatcher>(),
                    ctx.Resolve<ILogFactory>(),
                    ctx.ResolveEventPublishingService<TransactionFailedInBlockchainEvent>(),
                    ctx.ResolveEventPublishingService<MintEvent>(),
                    ctx.ResolveEventPublishingService<TransferEvent>(),
                    ctx.ResolveEventPublishingService<UndecodedEvent>(),
                    ctx.ResolveEventPublishingService<TransactionCompletedInBlockchainEvent>(),
                    ctx.ResolveEventPublishingService<StakeIncreasedEvent>(),
                    ctx.ResolveEventPublishingService<StakeReleasedEvent>(),
                    ctx.ResolveEventPublishingService<FeeCollectedEvent>(),
                    ctx.ResolveEventPublishingService<SeizedFromEvent>()
                ))
                .As<IShutdownManager>()
                .SingleInstance();

            builder
                .Register(ctx => new BlocksLagWatcher(
                    ctx.Resolve<ILogFactory>(),
                    ctx.Resolve<IBlockchainIndexingService>(),
                    blockchainSettings.WarningScanGapInBlocks,
                    blockchainSettings.ErrorScanGapInBlocks))
                .AsSelf()
                .SingleInstance();
        }
    }
}
