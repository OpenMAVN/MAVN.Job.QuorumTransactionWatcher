using System.Threading.Tasks;

namespace Lykke.Job.QuorumTransactionWatcher.Domain.Repositories
{
    public interface IIndexingStateRepository
    {
        Task<long?> GetLastIndexedBlockNumberAsync();

        Task SaveLastIndexedBlockNumber(long blockNumber);
    }
}
