using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Logs.Nethereum;
using Nethereum.Contracts.Services;
using Nethereum.JsonRpc.Client;
using Nethereum.JsonRpc.WebSocketClient;
using Nethereum.Web3;

namespace Lykke.Job.QuorumTransactionWatcher.DomainServices.Common
{
    public class EthNodeClientBase : IDisposable
    {
        private readonly Func<Task<string[]>> _connStringListGetter;
        private readonly ConcurrentDictionary<string, EthConnection> _connectionList = new ConcurrentDictionary<string, EthConnection>();
        private RoundRobinAccessor<IEthApiContractService> _activeConnectionPool;
        private readonly TimerTrigger _timer;
        private readonly ILog _log;
        private readonly NethereumLog _nethereumLog;

        private const int DefaultWebSocketConnectionTimeoutInSeconds = 20;

        public EthNodeClientBase(Func<Task<string[]>> connStringListGetter, ILogFactory log, TimeSpan? webSocketConnectionTimeout)
        {
            _connStringListGetter = connStringListGetter;
            _log = log.CreateLog(this);
            _nethereumLog = log.CreateNethereumLog(this);
            UpdateConnectionList(connStringListGetter.Invoke().Result);
            SetActiveConnectionPool();
            _timer = new TimerTrigger(GetType().Name, TimeSpan.FromSeconds(300), log, TimerHandler);
            _timer.Start();
            
            // setting up timeout for websocket connections
            ClientBase.ConnectionTimeout = webSocketConnectionTimeout ?? TimeSpan.FromSeconds(DefaultWebSocketConnectionTimeoutInSeconds);
        }

        public int CountActiveConnections { get; private set; }

        protected IEthApiContractService EthApi()
        {
            if (_activeConnectionPool == null)
                throw new Exception("Do not have any active connection to blockchain");

            return _activeConnectionPool.GetNext();
        }

        private EthConnection InternalCreateInstanceApi(string connString, int index)
        {
            Web3 web3;
            if (connString.StartsWith("ws"))
            {
                var wsClient = new WebSocketClient(connString, log: _nethereumLog);
                web3 = new Web3(wsClient);
                return new EthConnection(connString, wsClient, web3, index);
            }

            web3 = new Web3(connString, _nethereumLog);
            return new EthConnection(connString, null, web3, index);
        }

        private async Task TimerHandler(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken cancellationtoken)
        {
            await DoUpdate();
        }

        protected async Task DoUpdate()
        {
            var connStrings = await _connStringListGetter.Invoke();
            var connectionListUpdated = UpdateConnectionList(connStrings);
            var connectionStatusUpdated = await UpdateActiveConnectionList();

            if (connectionListUpdated || connectionStatusUpdated)
            {
                SetActiveConnectionPool();
            }
        }

        private bool UpdateConnectionList(string[] connStringList)
        {
            var isChanged = false;

            for (var index = 0; index < connStringList.Length; index++)
            {
                if (!_connectionList.TryGetValue(connStringList[index], out var connection))
                {
                    connection = InternalCreateInstanceApi(connStringList[index], index);
                    _connectionList[connection.ConnectionString] = connection;
                    isChanged = true;
                }
            }

            foreach (var connectionString in _connectionList.Keys.ToArray())
            {
                if (!connStringList.Contains(connectionString))
                {
                    _connectionList.TryRemove(connectionString, out _);
                    isChanged = true;
                }

            }

            return isChanged;
        }

        private async Task<bool> UpdateActiveConnectionList()
        {
            var statusChanged = false;

            foreach (var connection in _connectionList.Values)
            {
                var isActive = await CheckConnection(connection);

                if (isActive == connection.IsActive)
                    continue;

                statusChanged = true;
                connection.IsActive = isActive;
            }

            return statusChanged;
        }

        private async Task<bool> CheckConnection(EthConnection connection)
        {
            try
            {
                var getBestExistingBlockNumber = await connection.EthApi.Blocks.GetBlockNumber.SendRequestAsync();
                return getBestExistingBlockNumber.Value > 0;
            }
            catch (Exception ex)
            {
                _log.Warning($"Detect death connection to BlockchainNode. Setting index: {connection.Index}", ex);
                return false;
            }
        }

        private void SetActiveConnectionPool()
        {
            var pool = _connectionList.Values.Where(c => c.IsActive).Select(c => c.EthApi).ToArray();
            if (!pool.Any())
            {
                var exception = new Exception("Do not found any active connection to blockchain");
                _log.Error(exception, "Do not found any active connection to blockchain");
                throw exception;
            }
            
            _activeConnectionPool = new RoundRobinAccessor<IEthApiContractService>(pool);

            CountActiveConnections = pool.Length;
            _log.Info($"Set new active block-chain connection pool. Capacity: {CountActiveConnections}");
        }

        private class EthConnection
        {
            public EthConnection(string connectionString, WebSocketClient wsClient, Web3 ethWeb3, int index)
            {
                ConnectionString = connectionString;
                WsClient = wsClient;
                EthWeb3 = ethWeb3;
                Index = index;
            }

            public bool IsActive { get; set; } = true;

            public string ConnectionString { get; }
            public WebSocketClient WsClient { get; }
            public Web3 EthWeb3 { get; }
            public int Index { get; }

            public IEthApiContractService EthApi
            {
                get
                {
                    return EthWeb3.Eth;
                }
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
