﻿using System;
using JetBrains.Annotations;

namespace Lykke.Job.QuorumTransactionWatcher.Contract
{
    [PublicAPI]
    public class TransactionFailedInBlockchainEvent : ITransactionEvent
    {
        public string TransactionHash { get; set; }

        /// <inheritdoc />
        public DateTime ObservedAt { get; set; }
    }
}
