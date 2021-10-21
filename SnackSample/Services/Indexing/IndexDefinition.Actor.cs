using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nest;
using SnackSample.Data;
using SnackSample.Services.Mappings;

namespace SnackSample.Services.Indexing
{
    public class ActorIndexDefinition : IndexDefinition
    {
        public override string Name => "actor";
        public override string EsType => ActorSearchItem.EsTypeName;

        public ActorIndexDefinition()
        {

        }

        public override Task<CreateIndexResponse> Create(IElasticClient client)
        {
            return client.Indices.CreateAsync(Name, i => i
                       .Settings(CommonIndexDescriptor)
                       .Map<ActorSearchItem>(m => m.AutoMap())
                       );
        }

        internal override int PerformIndexing(IElasticClient client, MoviesDbContext db, List<Guid> ids)
        {
            var movies = db.Actors
                .AsNoTracking()
                .Include(p => p.Movies)
                .Where(p => ids.Contains(p.Id))
                .AsEnumerable()
                .Select(ActorSearchItem.Map)
                .ToList();

            return PerformDocumentIndexing(client, movies);
        }

        internal override int PerformIndexing(IElasticClient client, MoviesDbContext db, int batchSize, int batchSkip = 0)
        {
            var movieIds = db.Actors
                .OrderBy(p => p.Id)
                .Skip(batchSkip)
                .Take(batchSize)
                .Select(p => p.Id)
                .ToList();

            return PerformIndexing(client, db, movieIds);
        }
    }
}
