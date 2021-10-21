using System.Threading.Tasks;
using FluentAssertions;
using SnackSample.ApiModels;
using Xunit;

namespace SnackSample.Tests
{
    public class SearchWithManagedDockerTests : IClassFixture<EnvironmentFixture>
    {
        public SearchWithManagedDockerTests(EnvironmentFixture fixture)
        {
            var settingsReader = new LaunchSettingsReader();
            LambdaFunction.Host = new LambdaHost(() => fixture.HostBuilder);
        }

        [Fact]
        public async Task TestSearchSuccess()
        {
            var request = new CommonSearchRequest()
            {
                Query = "ring",
                PageSize = 10
            };

            var lambdaFunction = new LambdaFunction();
            var results = await lambdaFunction.Search(request, null);

            results.TotalResults.Should().Be(7);
        }

        [Fact]
        public async Task TestIndexSuccess()
        {
            var lambda = new LambdaFunction();
            await lambda.IndexSourceData(null, null);
        }

    }
}