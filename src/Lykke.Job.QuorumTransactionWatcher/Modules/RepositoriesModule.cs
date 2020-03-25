using Autofac;
using JetBrains.Annotations;
using Lykke.Common.MsSql;
using Lykke.Job.QuorumTransactionWatcher.Domain.Repositories;
using Lykke.Job.QuorumTransactionWatcher.MsSqlRepositories;
using Lykke.Job.QuorumTransactionWatcher.MsSqlRepositories.Contexts;
using Lykke.Job.QuorumTransactionWatcher.Settings;
using Lykke.Job.QuorumTransactionWatcher.Settings.Job.Db;
using Lykke.SettingsReader;

namespace Lykke.Job.QuorumTransactionWatcher.Modules
{
    [UsedImplicitly]
    public class RepositoriesModule : Module
    {
        private readonly DbSettings _dbSettings;

        public RepositoriesModule(
            IReloadingManager<AppSettings> appSettings)
        {
            _dbSettings = appSettings.CurrentValue.QuorumTransactionWatcherJob.Db;
        }

        protected override void Load(
            ContainerBuilder builder)
        {
            builder.RegisterMsSql(
                _dbSettings.DataConnString,
                connString => new QtwContext(connString, false),
                dbConn => new QtwContext(dbConn));

            builder
                .Register(ctx => new IndexingStateRepository
                (
                    ctx.Resolve<MsSqlContextFactory<QtwContext>>()
                ))
                .As<IIndexingStateRepository>()
                .SingleInstance();
        }
    }
}
