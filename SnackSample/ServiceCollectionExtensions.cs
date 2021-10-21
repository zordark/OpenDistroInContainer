using System;
using System.Diagnostics;
using System.Text;
using Elasticsearch.Net;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace SnackSample
{
    public static class ServiceCollectionExtensions
    {
        public static void AddElasticClient(this IServiceCollection services, string nodeUrl)
        {
            var pool = new SingleNodeConnectionPool(new Uri(nodeUrl));

            var settings = new ConnectionSettings(pool)
                .ThrowExceptions(alwaysThrow: true)
                .ServerCertificateValidationCallback((sender, cert, chain, errors) => true)
                .PrettyJson()
                .EnableDebugMode()
                .MemoryStreamFactory(new MemoryStreamFactory())
                .DisableDirectStreaming();

            services.AddTransient<IElasticClient>(provider => new ElasticClient(settings));
        }
    }
}