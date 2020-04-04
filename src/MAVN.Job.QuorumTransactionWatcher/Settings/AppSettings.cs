using JetBrains.Annotations;
using MAVN.Job.QuorumTransactionWatcher.Settings.Job;
using Lykke.Sdk.Settings;

namespace MAVN.Job.QuorumTransactionWatcher.Settings
{
    [UsedImplicitly]
    public class AppSettings : BaseAppSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public QuorumTransactionWatcherJobSettings QuorumTransactionWatcherJob { get; set; }
    }
}
