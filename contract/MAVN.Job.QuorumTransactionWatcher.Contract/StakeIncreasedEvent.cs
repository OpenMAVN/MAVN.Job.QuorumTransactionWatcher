using System;
using MAVN.Numerics;

namespace MAVN.Job.QuorumTransactionWatcher.Contract
{
    public class StakeIncreasedEvent : ITransactionEvent
    {
        /// <summary>
        /// Wallet address of the customer
        /// </summary>
        public string WalletAddress { get; set; }

        /// <summary>
        /// Increased amount
        /// </summary>
        public Money18 Amount { get; set; }

        /// <summary>
        /// Hash of the transaction
        /// </summary>
        public string TransactionHash { get; set; }

        /// <summary>
        /// Timestamp of the event
        /// </summary>
        public DateTime ObservedAt { get; set; }
    }
}
