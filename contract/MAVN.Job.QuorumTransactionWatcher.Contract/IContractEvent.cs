using JetBrains.Annotations;

namespace MAVN.Job.QuorumTransactionWatcher.Contract
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
