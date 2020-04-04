using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Job.QuorumTransactionWatcher.Domain.Services;

namespace MAVN.Job.QuorumTransactionWatcher.Services
{
    public class BlocksLagWatcher : TimerPeriod
    {
        private readonly IBlockchainIndexingService _blockchainIndexingService;
        private readonly int _warningLevel;
        private readonly int _errorLevel;
        private readonly ILog _log;
        
        private const int DefaultRetryCount = 10;
        
        public BlocksLagWatcher(
            ILogFactory logFactory, 
            IBlockchainIndexingService blockchainIndexingService, 
            int warningLevel, 
            int errorLevel, 
            TimeSpan? idleTime = null)
            : base(idleTime ?? TimeSpan.FromSeconds(60), logFactory)
        {
            _blockchainIndexingService = blockchainIndexingService;
            _warningLevel = warningLevel;
            _errorLevel = errorLevel;
            _log = logFactory.CreateLog(this);
        }

        public override async Task Execute()
        {
            var lastKnownBlockNumber = await _blockchainIndexingService.GetLastKnownBlockAsync();
            
            var blockNumberToIndex = await _blockchainIndexingService.GetLastBlockFromDbAsync();

            var lag = lastKnownBlockNumber - blockNumberToIndex;

            if (lag >= _errorLevel)
            {
                _log.Error(message: "Too many unhandled blocks", context: new {lastKnownBlockNumber, blockNumberToIndex, lag});
                return;
            }

            if (lag >= _warningLevel)
            {
                _log.Warning("A lot of unhandled blocks", context: new {lastKnownBlockNumber, blockNumberToIndex, lag});
            }
        }
    }
}
