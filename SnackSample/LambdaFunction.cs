using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SnackSample.ApiModels;
using SnackSample.Data.Entity;
using SnackSample.Services;
using SnackSample.Services.Indexing;
using Movie = DM.MovieApi.MovieDb.Movies.Movie;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace SnackSample
{
    public class LambdaFunction
    {
        static LambdaFunction()
        {
            _host = new LambdaHost(LambdaHostBuilder.CreateHostBuilder);
        }

        private static LambdaHost _host;

        /// <summary>
        /// This is needed for tests only, to allow replace function host with mocked one
        /// </summary>
        internal static LambdaHost Host
        {
            get => _host;
            set => _host = value;
        }

        public async Task ReadSourceData(object request, ILambdaContext context)
        {
            var scope = _host.Provider.CreateScope();
            var fetchService = scope.ServiceProvider.GetService<IFetchDataService>();
            await fetchService!.FetchDataAsync();
        }

        public async Task IndexSourceData(object request, ILambdaContext context)
        {
            var scope = _host.Provider.CreateScope();
            var searchService = scope.ServiceProvider.GetService<ISearchService>();

            await searchService!.CreateIndex(IndexDefinition.Actor);
            await searchService!.CreateIndex(IndexDefinition.Movie);
            await searchService.FullRefresh();

            Debug.WriteLine("Indexing completed");
        }

        public async Task<SearchServiceResults> Search(CommonSearchRequest request, ILambdaContext context)
        {
            var scope = _host.Provider.CreateScope();
            var searchService = scope.ServiceProvider.GetService<ISearchService>();
            var results = await searchService!.Search(request);

            return results;
        }
    }
}
