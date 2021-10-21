using Microsoft.EntityFrameworkCore;
using SnackSample.Data.Entity;

namespace SnackSample.Data
{
    public class MoviesDbContext : DbContext
    {
        protected MoviesDbContext()
        {
        }

        public MoviesDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Actor> Actors { get; set; }
    }
}
