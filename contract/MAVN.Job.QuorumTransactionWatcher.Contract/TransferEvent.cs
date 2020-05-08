using MAVN.Numerics;
using JetBrains.Annotations;
using System;

namespace MAVN.Job.QuorumTransactionWatcher.Contract
{
    /// <summary>
    /// Event which is raised when p2p transfer is made
    /// </summary>
    [PublicAPI]
    public class TransferEvent : IContractEvent
    {
        /// <summary>
        /// BC address of the sender
        /// </summary>
        public string SourceAddress { get; set; }

        /// <summary>
        /// BC address of the receiver
        /// </summary>
        public string TargetAddress { get; set; }

        /// <summary>
        /// The Amount that is being transferred
        /// </summary>
        public Money18 Amount { get; set; }

        /// <inheritdoc />
        public string TransactionHash { get; set; }

        /// <inheritdoc />
        public int EventIndex { get; set; }

        /// <inheritdoc />
        public DateTime ObservedAt { get; set; }
    }
}
