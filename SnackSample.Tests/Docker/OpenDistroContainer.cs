using System.Collections.Generic;
using Docker.DotNet;
using Microsoft.Extensions.Logging;
using TestEnvironment.Docker;
using TestEnvironment.Docker.Containers.Elasticsearch;

namespace SnackSample.Tests.Docker
{
    public class OpenDistroContainer : Container
    {
        public OpenDistroContainer(
            DockerClient dockerClient,
            string name,
            string imageName = "amazon/opendistro-for-elasticsearch",
            string tag = "1.13.2",
            IDictionary<string, string> environmentVariables = null,
            IDictionary<ushort, ushort> ports = null,
            bool isDockerInDocker = false,
            bool reuseContainer = false,
            ILogger logger = null
            ) : base(
                dockerClient,
                name,
                imageName,
                tag,
                new Dictionary<string, string>
                {
                    ["discovery.type"] = "single-node",
                    ["cluster.name"] = "test-cluster",
                    ["ES_JAVA_OPTS"] = "-Xms512m -Xmx512m",
                }.MergeDictionaries(environmentVariables),
                ports,
                isDockerInDocker,
                reuseContainer,
                new OpenDistroContainerWaiter(logger),
                new OpenDistroContainerCleaner(),
                logger)
        {
        }

        public string GetUrl() => IsDockerInDocker ? $"https://{IPAddress}:9200" : $"https://admin:admin@{System.Net.IPAddress.Loopback}:{Ports[9200]}";
    }
}