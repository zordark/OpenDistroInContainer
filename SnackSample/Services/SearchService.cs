using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using SnackSample.ApiModels;
using SnackSample.Data;
using SnackSample.Services.Indexing;
using SnackSample.Services.Mappings;
using SnackSample.Services.Mappings.NestedTypes;

namespace SnackSample.Services
{
    public interface ISearchService
    {
        Task FullRefresh();
        Task DeleteIndexIfExists(IndexDefinition index);
        Task CreateIndex(IndexDefinition index, bool autoReindex = true);
        ReindexResponse Reindex(IndexDefinition index, List<Guid> ids = null);
        Task<SearchServiceResults> Search(CommonSearchRequest searchRequest);
    }

    public class SearchService : ISearchService
    {
        private IElasticClient _client;
        private MoviesDbContext _dataContext;

        public SearchService(IElasticClient elasticClient, MoviesDbContext dataContext)
        {
            this._client = elasticClient;
            this._dataContext = dataContext;
        }

        public async Task FullRefresh()
        {
            await _client.Indices.RefreshAsync(IndexDefinition.All.Select(p => p.Name).ToArray());
        }

        public async Task DeleteIndexIfExists(IndexDefinition index)
        {
            var exists = await _client.Indices.ExistsAsync(index.Name);
            if (exists.Exists)
                await _client.Indices.DeleteAsync(index.Name);
        }

        public async Task CreateIndex(IndexDefinition index, bool autoReindex = true)
        {
            await DeleteIndexIfExists(index);

            var createIndexResponse = await index.Create(_client);

            if (autoReindex)
            {
                Reindex(index);
            }
        }

        public ReindexResponse Reindex(IndexDefinition index, List<Guid> ids = null)
        {
            ReindexResponse reindexResponse = new ReindexResponse();

            while (true)
            {
                if (ids == null || !ids.Any())
                {
                    const int batchSize = 100;

                    var processed = index.PerformIndexing(_client, _dataContext, batchSize: batchSize, batchSkip: reindexResponse.TotalProcessed);
                    reindexResponse.TotalProcessed += processed;

                    if (processed < batchSize)
                    {
                        break;
                    }
                }
                else
                {
                    reindexResponse.TotalProcessed += index.PerformIndexing(_client, _dataContext, ids);
                    break;
                }
            }

            return reindexResponse;
        }

        private static Type FromStringType(string esType)
        {
            var type = typeof(SearchItemDocumentBase).Assembly
                .GetTypes()
                .Where(t => typeof(SearchItemDocumentBase)
                    .IsAssignableFrom(t))
                .Single(t => t.GetCustomAttributes(inherit: true).OfType<ElasticsearchTypeAttribute>()
                    .Any(a => a.Name == esType));

            return type;
        }

        public async Task<SearchServiceResults> Search(CommonSearchRequest searchRequest)
        {
            var filteringQuery = CreateCommonFilter(searchRequest);

            const int titleBoost = 15;
            const int keywordBoost = 45;
            const int castBoost = 20;

            var results = await _client.SearchAsync<SearchItemDocumentBase>(s => s
                .From(searchRequest.Skip)
                .Size(searchRequest.PageSize)
                .Index(Indices.Index(IndexDefinition.All.Select(p => p.Name).ToArray()))
                .Query(q => q
                    .FunctionScore(fsc => fsc
                        .BoostMode(FunctionBoostMode.Multiply)
                        .ScoreMode(FunctionScoreMode.Sum)
                        .Functions(f => f
                            .FieldValueFactor(b => b
                                .Field(nameof(SearchItemDocumentBase.Rating))
                                .Missing(0.7)
                                .Modifier(FieldValueFactorModifier.None)
                            )
                        )
                        .Query(qx => qx.MultiMatch(m => m
                            .Query(searchRequest.Query.ToLower())
                            .Fields(ff => ff
                                .Field(f => f.Title, boost: titleBoost)
                                .Field(f => f.Summary)
                                .Field(f => f.Keywords, boost: keywordBoost)
                                .Field($"{nameof(MovieSearchItem.Cast)}.{nameof(ActorNestedType.Name)}",
                                    boost: castBoost)
                            )
                            .Type(TextQueryType.BestFields)
                        ) && filteringQuery)
                    )
                )
                .Highlight(h => h
                    .Fields(ff => ff
                        .Field(f => f.Title)
                        .Field(f => f.Summary)
                        .NumberOfFragments(2)
                        .FragmentSize(250)
                        .NoMatchSize(200)
                    )
                )
            );

            var searchResult = new SearchServiceResults()
            {
                TotalResults = (int)results.Total,
                DebugInformation = results.DebugInformation,
                OriginalQuery = searchRequest.Query
            };

            foreach (var hit in results.Hits)
            {
                var relatedDocument = results.Documents.FirstOrDefault(p => p.Id == hit.Id);
                relatedDocument.PostProcess(hit.Highlight);
                searchResult.Items.Add(relatedDocument);
            }

            return searchResult;
        }

        private BoolQuery CreateCommonFilter(CommonSearchRequest searchRequest)
        {
            var filteringQuery = new BoolQuery();

            var requiredQueryParts = new List<QueryContainer>();

            if (searchRequest.ActorIds != null && searchRequest.ActorIds.Any())
            {
                requiredQueryParts.Add(GetOptionalFieldQuery(new TermsQuery()
                {
                    Field = $"{nameof(MovieSearchItem.Cast)}.{nameof(ActorNestedType.Id)}",
                    Terms = searchRequest.ActorIds.Cast<object>().ToList()
                }));
            }

            if (searchRequest.DateFrom != null || searchRequest.DateTo != null)
            {
                var dateRange = new DateRangeQuery()
                {
                    Field = nameof(MovieSearchItem.Date)
                };

                if (searchRequest.DateFrom != null)
                {
                    dateRange.GreaterThanOrEqualTo = searchRequest.DateFrom;
                }

                if (searchRequest.DateTo != null)
                {
                    dateRange.LessThanOrEqualTo = searchRequest.DateTo;
                }

                requiredQueryParts.Add(GetOptionalFieldQuery(dateRange));
            }

            filteringQuery.Filter = requiredQueryParts;

            return filteringQuery;
        }

        private BoolQuery GetOptionalFieldQuery(FieldNameQueryBase fieldNameQuery)
        {
            return new BoolQuery()
            {
                Should = new QueryContainer[]
                {
                    new BoolQuery()
                    {
                        MustNot = new QueryContainer[] {
                            new ExistsQuery()
                            {
                                Field = fieldNameQuery.Field
                            }
                        }
                    },
                    fieldNameQuery
                },
                MinimumShouldMatch = 1
            };
        }
    }
}
