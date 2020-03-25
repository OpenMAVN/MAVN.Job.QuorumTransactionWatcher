using System;

namespace Lykke.Job.QuorumTransactionWatcher.Contract
{
    public class TransactionCompletedInBlockchainEvent : ITransactionEvent
    {
        public string TransactionHash { get; set; }
        public DateTime ObservedAt { get; set; }
    }
}
