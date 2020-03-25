using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.QuorumTransactionWatcher.Domain.Services;

namespace Lykke.Job.QuorumTransactionWatcher.Services
{
    public class IndexingService : TimerPeriod
    {
        private readonly IBlockchainIndexingService _blockchainIndexingService;
        private readonly ILog _log; 

        public IndexingService(
            IBlockchainIndexingService blockchainIndexingService,
            ILogFactory logFactory,
            TimeSpan? idleTime = null)
            : base(idleTime ?? TimeSpan.FromSeconds(5), logFactory)
        {
            _blockchainIndexingService = blockchainIndexingService;
            _log = logFactory.CreateLog(this);
        }

        public override async Task Execute()
        {
            try
            {
                await _blockchainIndexingService.IndexUntilLastBlockAsync();
            }
            catch (Exception e)
            {
                _log.Error(e, e.Message);
            }
        }
    }
}
