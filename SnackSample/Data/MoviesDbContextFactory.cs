using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SnackSample.Data
{
    public class MoviesDbContextFactory : IDesignTimeDbContextFactory<MoviesDbContext>
    {
        public MoviesDbContext CreateDbContext(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddUserSecrets(typeof(MoviesDbContextFactory).Assembly);

            var config = configurationBuilder.Build();
            var connectionString = config["DbConnection"];

            var optionsBuilder = new DbContextOptionsBuilder<MoviesDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new MoviesDbContext(optionsBuilder.Options);
        }
    }
}