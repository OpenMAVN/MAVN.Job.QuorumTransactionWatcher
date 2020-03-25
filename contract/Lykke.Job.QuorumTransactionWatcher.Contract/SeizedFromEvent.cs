using System;
using Falcon.Numerics;

namespace Lykke.Job.QuorumTransactionWatcher.Contract
{
    public class SeizedFromEvent : ITransactionEvent
    {
        public string TransactionHash { get; set; }
        public DateTime ObservedAt { get; set; }
        public string Address { get; set; }
        public Money18 Amount { get; set; }
    }
}
