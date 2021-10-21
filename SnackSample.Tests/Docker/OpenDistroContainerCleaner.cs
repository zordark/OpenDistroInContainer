using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nest;
using TestEnvironment.Docker;
using TestEnvironment.Docker.Containers.Elasticsearch;

namespace SnackSample.Tests.Docker
{
    public class OpenDistroContainerCleaner : IContainerCleaner<OpenDistroContainer>, IContainerCleaner
    {
        public async Task Cleanup(OpenDistroContainer container, CancellationToken token = default(CancellationToken))
        {
            ElasticClient elastic = container != null ? new ElasticClient(new Uri(container.GetUrl())) : throw new ArgumentNullException(nameof(container));
            DeleteIndexTemplateResponse templateResponse = await elastic.Indices.DeleteTemplateAsync((Name)"*", ct: token);
            DeleteIndexResponse deleteIndexResponse = await elastic.Indices.DeleteAsync((Indices)"*", ct: token);
            elastic = (ElasticClient)null;
        }

        public Task Cleanup(Container container, CancellationToken token = default(CancellationToken)) => this.Cleanup((OpenDistroContainer)container, token);
    }
}
