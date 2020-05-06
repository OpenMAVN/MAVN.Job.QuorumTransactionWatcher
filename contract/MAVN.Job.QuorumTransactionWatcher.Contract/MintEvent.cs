using System;
using MAVN.Numerics;
using JetBrains.Annotations;

namespace MAVN.Job.QuorumTransactionWatcher.Contract
{
    /// <summary>
    /// Event which is triggered when tokens are minted for a customer
    /// </summary>
    [PublicAPI]
    public class MintEvent : IContractEvent
    {
        /// <inheritdoc cref="ITransactionEvent" />
        public string TransactionHash { get; set; }

        /// <inheritdoc />
        public DateTime ObservedAt { get; set; }

        /// <inheritdoc cref="IContractEvent" />
        public int EventIndex { get; set; }

        /// <summary>
        /// Address which received the tokens
        /// </summary>
        public string TargetAddress { get; set; }

        /// <summary>
        /// Amount of minted tokens
        /// </summary>
        public Money18 Amount { get; set; }
    }
}
