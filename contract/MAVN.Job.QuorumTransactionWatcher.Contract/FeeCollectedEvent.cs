using System;
using MAVN.Numerics;

namespace MAVN.Job.QuorumTransactionWatcher.Contract
{
    public class FeeCollectedEvent : ITransactionEvent
    {
        /// <summary>
        /// Unique identifier of the event
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// Internal wallet address of the customer who paid the fee
        /// </summary>
        public string WalletAddress { get; set; }
        
        /// <summary>
        /// Amount of tokens
        /// </summary>
        public Money18 Amount { get; set; }

        /// <summary>
        /// Reason for collecting the fee
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Hash of the transaction
        /// </summary>
        public string TransactionHash { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime ObservedAt { get; set; }
    }
}
