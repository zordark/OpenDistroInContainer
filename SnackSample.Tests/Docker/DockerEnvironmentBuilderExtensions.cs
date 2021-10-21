using System.Collections.Generic;
using TestEnvironment.Docker;

namespace SnackSample.Tests.Docker
{
    public static class DockerEnvironmentBuilderExtensions
    {
        public static IDockerEnvironmentBuilder AddOpenDistroContainer(
            this IDockerEnvironmentBuilder builder, string name, string imageName = "amazon/opendistro-for-elasticsearch", string tag = "1.13.2", IDictionary<string, string> environmentVariables = null, IDictionary<ushort, ushort> ports = null, bool reuseContainer = false) =>
            builder.AddDependency(
                new OpenDistroContainer(builder.DockerClient, name.GetContainerName(builder.EnvironmentName), imageName, tag, environmentVariables, ports, builder.IsDockerInDocker, reuseContainer, builder.Logger));
    }
}
