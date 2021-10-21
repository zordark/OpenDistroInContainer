using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DM.MovieApi.MovieDb.Movies;
using DM.MovieApi.MovieDb.People;
using SnackSample.Data;
using SnackSample.Data.Entity;
using Movie = SnackSample.Data.Entity.Movie;

namespace SnackSample.Services
{
    class FetchDataService : IFetchDataService
    {
        private readonly IApiMovieRequest _movieApi;
        private readonly IApiPeopleRequest _peopleApi;
        private readonly MoviesDbContext _dbContext;

        public FetchDataService(IApiMovieRequest movieApi, IApiPeopleRequest peopleApi, MoviesDbContext dbContext)
        {
            _movieApi = movieApi;
            _peopleApi = peopleApi;
            _dbContext = dbContext;
        }

        public async Task FetchDataAsync()
        {
            for (int i = 1, count = 1000; i <= count; i++)
            {
                var movieApiResponse = await _movieApi.GetTopRatedAsync(i);
                count = movieApiResponse.TotalPages;

                foreach (var movieInfo in movieApiResponse.Results)
                {
                    Thread.Sleep(250);

                    var movieDb = _dbContext.Movies.FirstOrDefault(p => p.ApiId == movieInfo.Id);

                    if (movieDb == null)
                    {
                        movieDb = new Movie
                        {
                            ApiId = movieInfo.Id,
                            Cast = new List<Actor>(),
                            Title = movieInfo.Title,
                            Summary = movieInfo.Overview,
                            AirDate = movieInfo.ReleaseDate,
                            Rating = movieInfo.VoteAverage
                        };

                        var response = await _movieApi.GetCreditsAsync(movieInfo.Id);
                        var credits = response.Item;

                        foreach (var castMember in credits.CastMembers.Take(10))
                        {
                            var actor = await GetOrCreateActor(castMember.CastId);
                            if (actor == null) continue;

                            movieDb.Cast.Add(actor);
                            Thread.Sleep(250);
                        }

                        _dbContext.Movies.Add(movieDb);
                        await _dbContext.SaveChangesAsync();

                        Debug.WriteLine("[Added] " + movieDb.Title);
                    }
                    else
                    {
                        Debug.WriteLine("[Exists] " + movieDb.Title);
                    }
                }

            }
        }

        private async Task<Actor> GetOrCreateActor(int id)
        {
            var personResponse = await _peopleApi.FindByIdAsync(id);
            if (personResponse.Error?.StatusCode == 34)
                return null;

            var person = personResponse.Item;

            var actor = _dbContext.Actors.FirstOrDefault(p => p.ApiId == id);

            if (actor == null)
            {
                actor = new Actor { ApiId = person.Id, FullName = person.Name, Bio = person.Biography };
                _dbContext.Actors.Add(actor);
            }

            return actor;
        }
    }
}
