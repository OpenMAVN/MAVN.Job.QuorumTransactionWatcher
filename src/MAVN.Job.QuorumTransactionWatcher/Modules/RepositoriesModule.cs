using Autofac;
using JetBrains.Annotations;
using MAVN.Job.QuorumTransactionWatcher.Domain.Repositories;
using MAVN.Job.QuorumTransactionWatcher.MsSqlRepositories;
using MAVN.Job.QuorumTransactionWatcher.MsSqlRepositories.Contexts;
using MAVN.Job.QuorumTransactionWatcher.Settings;
using MAVN.Job.QuorumTransactionWatcher.Settings.Job.Db;
using Lykke.SettingsReader;
using MAVN.Persistence.PostgreSQL.Legacy;

namespace MAVN.Job.QuorumTransactionWatcher.Modules
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
            builder.RegisterPostgreSQL(
                _dbSettings.DataConnString,
                connString => new QtwContext(connString, false),
                dbConn => new QtwContext(dbConn));

            builder
                .Register(ctx => new IndexingStateRepository
                (
                    ctx.Resolve<PostgreSQLContextFactory<QtwContext>>()
                ))
                .As<IIndexingStateRepository>()
                .SingleInstance();
        }
    }
}
