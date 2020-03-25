using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.QuorumTransactionWatcher.Settings.Job.Rabbit
{
    [UsedImplicitly]
    public class PublisherSettings
    {
        [AmqpCheck]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string ConnectionString { get; set; }
    }
}
