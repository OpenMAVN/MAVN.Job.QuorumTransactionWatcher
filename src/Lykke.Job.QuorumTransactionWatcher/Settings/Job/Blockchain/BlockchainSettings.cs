using System;
using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.QuorumTransactionWatcher.Settings.Job.Blockchain
{
    [UsedImplicitly]
    public class BlockchainSettings
    {
        public string[] TransactionNodeUrlList { get; set; }

        [Optional]
        public int? BatchSize { get; set; }

        [Optional]
        public TimeSpan? IdleTime { get; set; }

        [Optional]
        public TimeSpan? WebSocketsConnectionTimeOut { get; set; }

        public int WarningScanGapInBlocks { get; set; }

        public int ErrorScanGapInBlocks { get; set; }

        public ContractAddressesSettings ContractAddresses { get; set; }
    }
}
