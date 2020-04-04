using System;
using System.Collections.Generic;

namespace MAVN.Job.QuorumTransactionWatcher.Contract
{
    public static class Context
    {
        private static readonly Dictionary<Type, string> EventsEndpointNames = new Dictionary<Type, string>
        {
            {typeof(MintEvent),"private-blockchain.mint" },
            {typeof(CustomerRegisteredInBlockchainEvent),"private-blockchain.customer-registered" },
            {typeof(TransactionFailedInBlockchainEvent),"private-blockchain.transaction-failed" },
            {typeof(TransferEvent),"private-blockchain.transfer" },
            {typeof(TransactionCompletedInBlockchainEvent),"private-blockchain.transaction-completed" },
            {typeof(UndecodedEvent),"private-blockchain.undecoded" },
            {typeof(StakeIncreasedEvent),"private-blockchain.stake-increased" },
            {typeof(StakeReleasedEvent),"private-blockchain.stake-released" },
            {typeof(FeeCollectedEvent),"private-blockchain.fee-collected" },
            {typeof(SeizedFromEvent),"private-blockchain.seized-from" },
        };

        public static string GetEndpointName<T>()
            where T : ITransactionEvent
        {
            var type = typeof(T);

            var endpointNameRegistered = EventsEndpointNames.TryGetValue(type, out var endpointName);

            if (endpointNameRegistered)
                return endpointName;

            throw new ArgumentOutOfRangeException($"Endpoint name for {type.Name} is not specified.");
        }
    }
}
