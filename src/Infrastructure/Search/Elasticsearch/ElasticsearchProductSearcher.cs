using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using eShopX.Application.Exceptions;
using Infrastructure.Search.Embedding;

namespace Infrastructure.Search.Elasticsearch;

public class ElasticsearchProductSearcher(
    ElasticsearchClient esClient,
    IOptions<ElasticsearchOptions> options,
    IEmbeddingClient embeddingClient) : IProductSearcher
{
    private readonly string _index = options.Value.IndexName;

    public async Task<Result<ProductSearchResponse>> SearchAsync(ProductSearchQuery query, CancellationToken cancellationToken = default)
    {
        int page = query.Page > 0 ? query.Page : 1,
            size = query.PageSize > 0 ? Math.Min(query.PageSize, 50) : 10,
            from = (page - 1) * size;

        var filters = new List<Query>();
        if (query.IsActive.HasValue)
            filters.Add(new TermQuery("isActive", query.IsActive.Value));

        if (query.MinPrice.HasValue || query.MaxPrice.HasValue)
            filters.Add(new NumberRangeQuery("price")
            {
                Gte = query.MinPrice.HasValue ? (double)query.MinPrice.Value : null,
                Lte = query.MaxPrice.HasValue ? (double)query.MaxPrice.Value : null
            });

        if (query.CategoryId.HasValue)
            filters.Add(new TermQuery("categoryId.keyword", query.CategoryId.Value.ToString()));

        if (query.Audience.HasValue)
            filters.Add(new BoolQuery
            {
                Should =
                [
                    new TermQuery("audience.keyword", query.Audience.Value.ToString()),
                    new BoolQuery { MustNot = [new ExistsQuery { Field = "audience" }] }
                ],
                MinimumShouldMatch = 1
            });

        var hasKeyword = !string.IsNullOrWhiteSpace(query.Keyword);

        if (hasKeyword)
        {
            // Hybrid: kNN (semantic) + BM25 (keyword), scores combined by ES
            var queryVector = await embeddingClient.GetEmbeddingAsync(query.Keyword!, cancellationToken);

            var response = await esClient.SearchAsync<ProductSearchDocument>(s => s
                .Indices(_index)
                .From(from)
                .Size(size)
                .Source(src => src.Filter(f => f.Excludes(x => x.Embedding)))
                .Knn(k => k
                    .Field(f => f.Embedding)
                    .QueryVector(queryVector)
                    .K(size * 5)
                    .NumCandidates(Math.Max(100, size * 5))
                    .Filter(filters.ToArray()))
                .Query(q => q.Bool(b => b
                    .Must(new MultiMatchQuery { Query = query.Keyword!, Fields = Infer.Fields<ProductSearchDocument>(f => f.Name, f => f.Description) })
                    .Filter(filters.ToArray()))),
                cancellationToken);

            if (!response.IsValidResponse)
                throw new ExternalServiceException("Elasticsearch",
                    response.ElasticsearchServerError?.Error?.Reason ?? response.DebugInformation);

            return Result<ProductSearchResponse>.Ok(BuildResponse(response, page, size));
        }
        else
        {
            // No keyword: filter only, sorted by createdAt
            var response = await esClient.SearchAsync<ProductSearchDocument>(s => s
                .Indices(_index)
                .From(from)
                .Size(size)
                .Source(src => src.Filter(f => f.Excludes(x => x.Embedding)))
                .Query(q => q.Bool(b => b
                    .Must(new MatchAllQuery())
                    .Filter(filters.ToArray())))
                .Sort(so => so.Field(f => f.Field("createdAt").Order(SortOrder.Desc))),
                cancellationToken);

            if (!response.IsValidResponse)
                throw new ExternalServiceException("Elasticsearch",
                    response.ElasticsearchServerError?.Error?.Reason ?? response.DebugInformation);
            
            return Result<ProductSearchResponse>.Ok(BuildResponse(response, page, size));
        }
    }

    private static ProductSearchResponse BuildResponse(SearchResponse<ProductSearchDocument> response, int page, int size)
    {
        var total = (int)(response.HitsMetadata?.Total?.Match(
            totalHits => totalHits?.Value,
            totalAsLong => totalAsLong) ?? 0L);

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling((double)total / size);

        var items = response.Documents.Select(d => new ProductSearchItem(
            d.ProductId,
            d.CategoryId,
            d.Name,
            d.Description,
            d.Price,
            d.StockQuantity,
            d.IsActive,
            d.PrimaryImageUrl
        )).ToList();

        return new ProductSearchResponse(page, size, total, totalPages, items);
    }
}

public class ProductSearchDocument
{
    public Guid ProductId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public string? Audience { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public float[] Embedding { get; set; } = [];
}
