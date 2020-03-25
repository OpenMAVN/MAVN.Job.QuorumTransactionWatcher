using JetBrains.Annotations;
using Lykke.Job.QuorumTransactionWatcher.Settings.Job;
using Lykke.Sdk.Settings;

namespace Lykke.Job.QuorumTransactionWatcher.Settings
{
    [UsedImplicitly]
    public class AppSettings : BaseAppSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public QuorumTransactionWatcherJobSettings QuorumTransactionWatcherJob { get; set; }
    }
}
