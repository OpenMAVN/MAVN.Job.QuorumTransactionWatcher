using System.Threading.Tasks;

namespace MAVN.Job.QuorumTransactionWatcher.Domain.Services
{
    public interface IBlockchainIndexingService
    {
        Task IndexUntilLastBlockAsync();

        Task<long> GetLastBlockFromDbAsync();

        Task<long> GetLastKnownBlockAsync();
    }
}
