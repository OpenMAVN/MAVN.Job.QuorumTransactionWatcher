using System;

namespace Lykke.Job.QuorumTransactionWatcher.Contract
{
    /// <summary>
    /// Event which holds information for undecoded blockchain event
    /// </summary>
    public class UndecodedEvent : ITransactionEvent
    {
        /// <summary>
        /// Unique identifier of the event
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Hash of the transaction
        /// </summary>
        public string TransactionHash { get; set; }
        /// <summary>
        /// Timestamp when the event was observed
        /// </summary>
        public DateTime ObservedAt { get; set; }
        /// <summary>
        /// The index of the event in a transaction scope
        /// </summary>
        public string Index { get; set; }
        /// <summary>
        /// Log address which is used to get the origin of the event
        /// </summary>
        public string OriginAddress { get; set; }
        /// <summary>
        /// Transaction log data
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// Transaction log topics
        /// </summary>
        public string[] Topics { get; set; }
    }
}
