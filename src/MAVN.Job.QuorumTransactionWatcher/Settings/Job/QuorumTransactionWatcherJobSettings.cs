using JetBrains.Annotations;
using MAVN.Job.QuorumTransactionWatcher.Settings.Job.Blockchain;
using MAVN.Job.QuorumTransactionWatcher.Settings.Job.Db;
using MAVN.Job.QuorumTransactionWatcher.Settings.Job.Rabbit;

namespace MAVN.Job.QuorumTransactionWatcher.Settings.Job
{
    [UsedImplicitly]
    public class QuorumTransactionWatcherJobSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public BlockchainSettings Blockchain { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public DbSettings Db { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public PublisherSettings Publisher { get; set; }
    }
}
