using System;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Nest;
using TestEnvironment.Docker;

namespace SnackSample.Tests.Docker
{
    public class OpenDistroContainerWaiter : BaseContainerWaiter<OpenDistroContainer>
    {
        public OpenDistroContainerWaiter(ILogger logger = null)
            : base(logger)
        {
        }

        protected override async Task<bool> PerformCheck(OpenDistroContainer container, CancellationToken cancellationToken)
        {
            var settings = new ConnectionSettings(new SingleNodeConnectionPool(new Uri(container.GetUrl())))
                .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
                .BasicAuthentication("admin","admin")
                .PrettyJson();

            var elastic = new ElasticClient(settings);
            
            var health = await elastic.Cluster.HealthAsync(
                selector: ch => ch
                    .WaitForStatus(WaitForStatus.Yellow)
                    .Level(Level.Cluster)
                    .ErrorTrace(true),
                ct: cancellationToken);

            Logger?.LogDebug(health.DebugInformation);

            return health.IsValid;
        }
    }
}