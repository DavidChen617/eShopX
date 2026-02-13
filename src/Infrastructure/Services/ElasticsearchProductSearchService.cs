using ApplicationCore.UseCases.Products.GetProducts;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using eShopX.Common.Exceptions;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class ElasticsearchProductSearchService(
    ElasticsearchClient esClient,
    IOptions<ElasticsearchOptions> options): IProductSearchService
{
    private readonly string _index = options.Value.IndexName;
    
    public async Task<GetProductResponse> SearchAsync(GetProductsQuery query, CancellationToken cancellationToken = default)
    {
        int page = query.Page is > 0 ? query.Page.Value : 1,
            size = query.PageSize is > 0? Math.Min(query.PageSize.Value, 50): 10,
            from = (page - 1)  * size;

        var filters = new List<Query>();
        if (query.IsActive.HasValue)
            filters.Add(new TermQuery("isActive", query.IsActive.Value));

        if (query.MinPrice.HasValue || query.MaxPrice.HasValue)
        {
            filters.Add(new NumberRangeQuery("price")
            {
                Gte = query.MinPrice.HasValue? (double)query.MinPrice.Value: null,
                Lte = query.MaxPrice.HasValue? (double)query.MaxPrice.Value: null,
            });
        }
        
        if(query.CategoryId.HasValue)
            filters.Add(new TermQuery("categoryId", query.CategoryId.Value.ToString()));
        
        if(query.SellerId.HasValue)
            filters.Add(new TermQuery("sellerId", query.SellerId.Value.ToString()));

        Query mustQuery = string.IsNullOrWhiteSpace(query.Keyword)
            ? new MatchAllQuery()
            : new MultiMatchQuery() { Query = query.Keyword, Fields = new[] { "name^3", "description" } };

        var response = await esClient.SearchAsync<ProductSearchDocument>(s => s
            .Indices(_index)
            .From(from)
            .Size(size)
            .Query(q => 
                q.Bool(b => 
                    b.Must(mustQuery).Filter(filters.ToArray()
                    ))
                )
            .Sort(so => 
                so.Field(f => 
                    f.Field("createdAt").Order(SortOrder.Desc)
                    )
                ),
            cancellationToken);

        if (!response.IsValidResponse)
            throw new ExternalServiceException(
                $"Elasticsearch search failed: {response.ElasticsearchServerError?.Error?.Reason ?? response.DebugInformation}"
            );;
        
        var total = (int)(response.HitsMetadata?.Total?.Match(
            totalHits => totalHits?.Value,
            totalAsLong => totalAsLong
        ) ?? 0L);
        
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling((double)total / size);
        var items = response.Documents.Select(d => new GetProductItems(
            d.ProductId,
            d.CategoryId,
            d.Name,
            d.Description,
            d.Price,
            d.StockQuantity,
            d.IsActive,
            d.PrimaryImageUrl
        )).ToList();
        
        return new GetProductResponse(page, size, total, totalPages, items);
    }
}

public class ProductSearchDocument
{
    public Guid ProductId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? SellerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
