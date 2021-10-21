using System;
using System.Threading.Tasks;
using FluentAssertions;
using SnackSample.ApiModels;
using Xunit;

namespace SnackSample.Tests
{
    public class LambdaTests
    {
        public LambdaTests()
        {
            var settingsReader = new LaunchSettingsReader();
        }

        [Fact]
        public async Task TestFetchDataSuccess()
        {
            var lambda = new LambdaFunction();
            await lambda.ReadSourceData(null, null);
        }

        [Fact]
        public async Task TestIndexDataSuccess()
        {
            var lambda = new LambdaFunction();
            await lambda.IndexSourceData(null, null);
        }

        [Fact]
        public async Task TestSearchDataSuccess()
        {
            var lambda = new LambdaFunction();

            var request = new CommonSearchRequest()
            {
                Query = "ring",
                PageSize = 10
            };

            var result = await lambda.Search(request, null);
            result.TotalResults.Should().Be(7);
        }
    }
}
