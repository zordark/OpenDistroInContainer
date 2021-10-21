using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SnackSample.Tests.Docker;
using TestEnvironment.Docker;
using Xunit;

namespace SnackSample.Tests
{
    public class EnvironmentFixture : IAsyncLifetime
    {
        private readonly string _environmentName = "elasticSearch";
        private DockerEnvironment _environment;

        public IHostBuilder HostBuilder { get; private set; }

        private Dictionary<ushort, ushort> _elasticPorts = new Dictionary<ushort, ushort>()
        {
            {9200, 9201},
        };

        public async Task InitializeAsync()
        {
            // Docker environment setup.
            _environment = CreateTestEnvironmentBuilder().Build();
            await _environment.Up();

            // API Test host setup
            HostBuilder = CreateHostBuilder();
        }

        public async Task DisposeAsync()
        {
#if !DEBUG
            await _environment.DisposeAsync();
#endif
        }

        private IDockerEnvironmentBuilder CreateTestEnvironmentBuilder() =>
            new DockerEnvironmentBuilder()
                .SetName(_environmentName)
                .UseDefaultNetwork()
#if DEBUG
                .AddOpenDistroContainer("opendistro-xunit-tests", ports: _elasticPorts, reuseContainer: true);
#else
                .AddOpenDistroContainer("opendistro-nunit-tests", ports: _elasticPorts);
#endif

        private IHostBuilder CreateHostBuilder()
        {
            var opendistroContainer = _environment.GetContainer<OpenDistroContainer>("opendistro-xunit-tests");
            var builder = LambdaHostBuilder
                .CreateHostBuilder()
                .ConfigureAppConfiguration(cfgBuilder =>
                {
                    cfgBuilder.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>("esUrl", opendistroContainer.GetUrl())
                    });
                });

            return builder;
        }
    }
}
