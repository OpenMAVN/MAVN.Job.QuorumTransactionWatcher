using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MAVN.Job.QuorumTransactionWatcher.Settings.Job.Db
{
    [UsedImplicitly]
    public class DbSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string DataConnString { get; set; }

        [AzureTableCheck]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string LogsConnString { get; set; }
    }
}
