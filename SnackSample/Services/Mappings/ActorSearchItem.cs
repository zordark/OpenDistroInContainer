using System.Linq;
using Nest;
using SnackSample.Data.Entity;

namespace SnackSample.Services.Mappings
{
    [ElasticsearchType(IdProperty = nameof(SearchItemDocumentBase.Id), Name = ActorSearchItem.EsTypeName)]
    public class ActorSearchItem : SearchItemDocumentBase
    {
        public const string EsTypeName = "actorsearchitem";

        internal static ActorSearchItem Map(Actor actor)
        {
            var result = new ActorSearchItem()
            {
                Id = actor.Id.ToString(),
                Keywords = actor.FullName,
                Rating = actor.Movies.Average(p => p.Rating) * 0.1,
                Summary = actor.Bio,
                Title = actor.FullName
            };

            return result;
        }
    }
}
