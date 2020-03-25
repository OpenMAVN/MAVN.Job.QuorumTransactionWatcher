using System.Data.Common;
using JetBrains.Annotations;
using Lykke.Common.MsSql;
using Lykke.Job.QuorumTransactionWatcher.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Job.QuorumTransactionWatcher.MsSqlRepositories.Contexts
{
    public class QtwContext : MsSqlContext
    {
        private const string Schema = "quorum_transaction_watcher";

        internal DbSet<BlocksDataEntity> BlocksData { get; set; }

        // C-tor for EF migrations
        [UsedImplicitly]
        public QtwContext()
            : base(Schema)
        {
        }

        public QtwContext(string connectionString, bool isTraceEnabled)
            : base(Schema, connectionString, isTraceEnabled)
        {
        }

        public QtwContext(DbConnection dbConnection)
            : base(Schema, dbConnection)
        {
        }

        protected override void OnLykkeModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
