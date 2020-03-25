using JetBrains.Annotations;

namespace Lykke.Job.QuorumTransactionWatcher.Contract
{
    [PublicAPI]
    public interface IContractEvent : ITransactionEvent
    {
        /// <summary>
        ///    The event index within transaction.
        /// </summary>
        int EventIndex { get; set; }
    }
}
