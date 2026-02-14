using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class ReindexProductsService(
    IRepository<Product> productRepository,
    IRepository<ProductImage> productImageRepository,
    ElasticsearchClient esClient,
    IOptions<ElasticsearchOptions> options): IProductSearchIndexService
{
    private readonly string _index = options.Value.IndexName;
    
    public async Task<ReindexProductsResult> ReindexAsync(int batchSize = 500, CancellationToken cancellationToken = default)
    {
        batchSize = batchSize <= 0 ? 500 : Math.Min(batchSize, 2000);
        var allProducts = await productRepository.QueryAsync(q =>
            q.OrderBy(q => q.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.CategoryId,
                    p.SellerId,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.StockQuantity,
                    p.IsActive,
                    p.CreatedAt
                }), cancellationToken);
        var totalCount = allProducts.Count;
        
        if (totalCount == 0)
            return new ReindexProductsResult(0, 0, 0);

        var allProductIds = allProducts.Select(p => p.Id).ToList();
        var primaryImages =
            await productImageRepository.ListAsync(x => allProductIds.Contains(x.ProductId) && x.IsPrimary, cancellationToken);
        var primaryImageMap = primaryImages
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .GroupBy(x=> x.ProductId)
            .ToDictionary(k => k.Key, v => v.First().Url);
        var docs = allProducts.Select(p =>
        {
            primaryImageMap.TryGetValue(p.Id, out var imageUrl);
            return new ProductSearchDocument()
            {
                ProductId = p.Id,
                CategoryId = p.CategoryId,
                SellerId = p.SellerId,
                Name = p.Name,
                Description = p.Description ?? string.Empty,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive,
                PrimaryImageUrl = imageUrl,
                CreatedAt = p.CreatedAt
            };
        }).ToList();

        int indexed = 0,
            failed = 0;

        for (int i = 0; i < docs.Count; i += batchSize)
        {
            var chunk = docs.Skip(i).Take(batchSize).ToList();
            var operations = new List<IBulkOperation>(chunk.Count);
            
            foreach (var ch in chunk)
                operations.Add(new BulkIndexOperation<ProductSearchDocument>(ch)
                {
                    Id = ch.ProductId.ToString()
                });

            var bulk = await esClient.BulkAsync(new BulkRequest(_index) { Operations = operations }, cancellationToken);
            
            if (!bulk.IsValidResponse)
            {
                failed += chunk.Count;
                continue;
            }

            if (bulk.Errors)
            {
                var itemErrors = bulk.Items.Count(x => x.Error is not null);
                failed += itemErrors;
                indexed += chunk.Count - itemErrors;
            }
            else
                indexed += chunk.Count;

        }
        
        await esClient.Indices.RefreshAsync(_index, cancellationToken);
        return new ReindexProductsResult(totalCount, indexed, failed);
    }
}
