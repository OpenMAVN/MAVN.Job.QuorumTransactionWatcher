using System.Collections.Generic;
using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;

namespace Lykke.Job.QuorumTransactionWatcher.Domain.Services
{
    public interface IEthNodeClient
    {
        Task<long> GetBlockNumberAsync();

        Task<BlockWithTransactionHashes> GetBlockWithTransactionHashesAsync(long blockNumber);

        Task<IReadOnlyList<TransactionReceipt>> GetTransactionReceiptsAsync(BlockWithTransactionHashes blockWithTransaction);
    }
}
