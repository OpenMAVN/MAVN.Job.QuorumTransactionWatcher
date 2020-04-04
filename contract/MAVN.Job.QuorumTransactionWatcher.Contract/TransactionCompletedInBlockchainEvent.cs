using System;

namespace MAVN.Job.QuorumTransactionWatcher.Contract
{
    public class TransactionCompletedInBlockchainEvent : ITransactionEvent
    {
        public string TransactionHash { get; set; }
        public DateTime ObservedAt { get; set; }
    }
}
