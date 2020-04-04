using System.Threading.Tasks;

namespace MAVN.Job.QuorumTransactionWatcher.Domain.Repositories
{
    public interface IIndexingStateRepository
    {
        Task<long?> GetLastIndexedBlockNumberAsync();

        Task SaveLastIndexedBlockNumber(long blockNumber);
    }
}
