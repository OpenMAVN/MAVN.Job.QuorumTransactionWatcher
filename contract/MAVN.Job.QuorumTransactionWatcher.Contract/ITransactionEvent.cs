using System;
using JetBrains.Annotations;

namespace MAVN.Job.QuorumTransactionWatcher.Contract
{
    [PublicAPI]
    public interface ITransactionEvent
    {
        /// <summary>
        ///    The transaction hash.
        /// </summary>
        string TransactionHash { get; set; }

        /// <summary>
        /// Timestamp when the event was observed 
        /// </summary>
        DateTime ObservedAt { get; set; }
    }
}
