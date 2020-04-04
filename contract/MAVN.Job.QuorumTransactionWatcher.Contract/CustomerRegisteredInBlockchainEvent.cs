using System;
using JetBrains.Annotations;

namespace MAVN.Job.QuorumTransactionWatcher.Contract
{
    /// <summary>
    ///    Triggered when customer registration completed in blockchain customer registry.
    /// </summary>
    [PublicAPI]
    public class CustomerRegisteredInBlockchainEvent : IContractEvent
    {
        /// <summary>The customer id.</summary>
        public Guid CustomerId { get; set; }

        /// <summary>The wallet address.</summary>
        public string Address { get; set; }

        /// <inheritdoc cref="ITransactionEvent" />
        public string TransactionHash { get; set; }

        /// <inheritdoc />
        public DateTime ObservedAt { get; set; }

        /// <inheritdoc cref="IContractEvent" />
        public int EventIndex { get; set; }
    }
}
