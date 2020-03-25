using System.Threading.Tasks;

namespace Lykke.Job.QuorumTransactionWatcher.Domain.Services
{
    public interface IBlockchainIndexingService
    {
        Task IndexUntilLastBlockAsync();

        Task<long> GetLastBlockFromDbAsync();

        Task<long> GetLastKnownBlockAsync();
    }
}
