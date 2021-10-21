using System.IO;
using DM.MovieApi;
using DM.MovieApi.MovieDb.Movies;
using DM.MovieApi.MovieDb.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SnackSample.Data;
using SnackSample.Services;

namespace SnackSample
{
    public class LambdaHostBuilder
    {
        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(((context, builder) =>
                    {
                        builder
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                                optional: true);

                        if (context.HostingEnvironment.IsDevelopment())
                        {
                            builder.AddUserSecrets(typeof(LambdaHostBuilder).Assembly);
                        }

                        builder.AddEnvironmentVariables();
                    })
                )
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<MoviesDbContext>((provider, builder) =>
                    {
                        var connectionString = context.Configuration["DbConnection"];
                        SqlServerDbContextOptionsExtensions.UseSqlServer(builder, connectionString);
                    });

                    var elasticSearchUrl = context.Configuration["esUrl"];
                    services.AddElasticClient(elasticSearchUrl);

                    var apiKey = context.Configuration["ApiKey"];
                    MovieDbFactory.RegisterSettings(apiKey);
                    services.AddTransient<IApiMovieRequest>(provider =>
                        MovieDbFactory.Create<IApiMovieRequest>().Value);
                    services.AddTransient<IApiPeopleRequest>(provider =>
                        MovieDbFactory.Create<IApiPeopleRequest>().Value);

                    services.AddTransient<IFetchDataService, FetchDataService>();
                    services.AddTransient<ISearchService, SearchService>();

                });

    }
}