using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Common.Log;
using Lykke.Job.QuorumTransactionWatcher.Domain.Services;
using Lykke.Job.QuorumTransactionWatcher.DomainServices.Common;
using MoreLinq;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;

namespace Lykke.Job.QuorumTransactionWatcher.DomainServices
{
    public class EthNodeClient : EthNodeClientBase, IEthNodeClient
    {
        private const int MaxThreadsCount = 16;
        private readonly int _batchSize;

        public EthNodeClient(Func<Task<string[]>> connStringList, ILogFactory log, TimeSpan? webSocketConnectionTimeout, int? batchSize = null)
            :base(connStringList, log, webSocketConnectionTimeout)
        {
            _batchSize = batchSize ?? MaxThreadsCount;
        }

        public async Task<long> GetBlockNumberAsync()
        {
            var getBestExistingBlockNumber = await EthApi().Blocks.GetBlockNumber.SendRequestAsync();
            return (long)getBestExistingBlockNumber.Value;
        }

        public Task<BlockWithTransactionHashes> GetBlockWithTransactionHashesAsync(long blockNumber)
        {
            return EthApi().Blocks.GetBlockWithTransactionsHashesByNumber.SendRequestAsync(new HexBigInteger(blockNumber));
        }

        public async Task<IReadOnlyList<TransactionReceipt>> GetTransactionReceiptsAsync(BlockWithTransactionHashes blockWithTransaction)
        {
            var getTransactionReceiptBatches = blockWithTransaction.TransactionHashes.Batch(_batchSize);

            var result = new List<TransactionReceipt>();

            var api = EthApi();

            foreach (var transactionReceiptBatch in getTransactionReceiptBatches)
            {
                var dataFetchTasks = transactionReceiptBatch
                    .Select(x => api.Transactions.GetTransactionReceipt.SendRequestAsync(x))
                    .ToArray();

                await Task.WhenAll(dataFetchTasks);

                result.AddRange(dataFetchTasks.Select(x => x.Result));
            }

            return result;
        }
    }
}
