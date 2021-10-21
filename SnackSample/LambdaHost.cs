using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SnackSample
{
    /// <summary>
    /// Source data:            https://www.themoviedb.org/
    /// TestEnvironment.Docker: https://github.com/Deffiss/testenvironment-docker
    /// TestContainers:         https://github.com/testcontainers/testcontainers-dotnet
    ///                         https://www.testcontainers.org/modules/gcloud/
    /// </summary>
    public class LambdaHost
    {
        public LambdaHost(Func<IHostBuilder> createHostBuilderFunc)
            : this(createHostBuilderFunc, null)
        {
        }

        public LambdaHost(Func<IHostBuilder> createHostBuilderFunc, Action<HostBuilderContext, IServiceCollection> postConfigureDelegate)
        {
            var builder = createHostBuilderFunc();

            if (postConfigureDelegate != null)
                builder.ConfigureServices(postConfigureDelegate);

            this.host = builder.Build();
        }

        protected IHost host;

        public IServiceProvider Provider => host.Services;
    }
}