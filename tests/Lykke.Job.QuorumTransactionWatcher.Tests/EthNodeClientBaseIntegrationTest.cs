using System;
using System.Threading.Tasks;
using Lykke.Common.Log;
using Lykke.Job.QuorumTransactionWatcher.DomainServices.Common;
using Lykke.Logs;
using Nethereum.Contracts.Services;
using Xunit;
using Xunit.Abstractions;

namespace Lykke.Job.QuorumTransactionWatcher.Tests
{
    public class EthNodeClientBaseIntegrationTest
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private string[] _connStrings = { };
        private const int DefaultWebSocketConnectionTimeoutInSeconds = 20;

        public EthNodeClientBaseIntegrationTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact(Skip = "manual test")]
        public async Task TestRun()
        {
            _connStrings = new[]
            {
                "https://bnode-mavndevblockchain.blockchain.azure.com:3200/****"
            };

            var node = new Client(
                () => Task.FromResult(_connStrings), 
                EmptyLogFactory.Instance,
                TimeSpan.FromSeconds(DefaultWebSocketConnectionTimeoutInSeconds));

            await node.Update();

            _testOutputHelper.WriteLine($"Count connections: {node.CountActiveConnections}");

            var data = await node.Api.Blocks.GetBlockNumber.SendRequestAsync();
            _testOutputHelper.WriteLine($"Block number: {data.Value}");

            data = await node.Api.Blocks.GetBlockNumber.SendRequestAsync();
            _testOutputHelper.WriteLine($"Block number: {data.Value}");

            data = await node.Api.Blocks.GetBlockNumber.SendRequestAsync();
            _testOutputHelper.WriteLine($"Block number: {data.Value}");


            _connStrings = new[]
            {
                "https://mavndevblockchain.blockchain.azure.com:3200/***",
                "https://bnode-mavndevblockchain.blockchain.azure.com:3200/***"
            };
            await Task.Delay(2000);

            await node.Update();
            _testOutputHelper.WriteLine($"Count connections: {node.CountActiveConnections}");

            data = await node.Api.Blocks.GetBlockNumber.SendRequestAsync();
            _testOutputHelper.WriteLine($"Block number: {data.Value}");
            data = await node.Api.Blocks.GetBlockNumber.SendRequestAsync();
            _testOutputHelper.WriteLine($"Block number: {data.Value}");
            data = await node.Api.Blocks.GetBlockNumber.SendRequestAsync();
            _testOutputHelper.WriteLine($"Block number: {data.Value}");

            _testOutputHelper.WriteLine($"Test auto update");
            _connStrings = new[]
            {
                "https://mavndevblockchain.blockchain.azure.com:3200/***",
                "https://bnode-mavndevblockchain.blockchain.azure.com:3200/***"
            };
            await Task.Delay(7000);
            _testOutputHelper.WriteLine($"Count connections: {node.CountActiveConnections}");
            _connStrings = new[]
            {
                "https://mavndevblockchain.blockchain.azure.com:3200/***",
            };
            await Task.Delay(7000);
            _testOutputHelper.WriteLine($"Count connections: {node.CountActiveConnections}");
        }

        public class Client : EthNodeClientBase
        {
            public IEthApiContractService Api
            {
                get => this.EthApi();
            }

            public Client(Func<Task<string[]>> connStringList, ILogFactory log, TimeSpan? webSocketConnectionTimeout) :
                base(connStringList, log, webSocketConnectionTimeout)
            {
            }

            public Task Update()
            {
                return DoUpdate();
            }
        }
    }
}
