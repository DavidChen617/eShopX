using Elastic.Clients.Elasticsearch;
using eShopX.Common.Exceptions;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class ProductSearchIndexSyncService(
    IRepository<Product> productRepository,
    IRepository<ProductImage> productImageRepository,
    ElasticsearchClient esClient,
    IOptions<ElasticsearchOptions> options): IProductSearchIndexSyncService
{
    private readonly string _index = options.Value.IndexName;
    
    public async Task UpsertProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.QueryAsync(q =>
                q.Where(p => p.Id == productId)
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
                    }),
            cancellationToken);

        var p = product.FirstOrDefault();
        if(p is null)
            throw new NotFoundException(nameof(Product), productId);
        
        var primaryImage = await productImageRepository.QueryAsync(q => 
            q.Where(x => x.ProductId == p.Id && x.IsPrimary)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.CreatedAt)
                .Select(x => x.Url),
            cancellationToken);
        
        var doc = new ProductSearchDocument
        {
            ProductId = p.Id,
            CategoryId = p.CategoryId,
            SellerId = p.SellerId,
            Name = p.Name,
            Description = p.Description ?? string.Empty,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            IsActive = p.IsActive,
            PrimaryImageUrl = primaryImage.FirstOrDefault(),
            CreatedAt = p.CreatedAt
        };

        var response = await esClient.IndexAsync(doc, 
            i => i.Index(_index).Id(doc.ProductId.ToString())
        , cancellationToken);
        
        if(!response.IsValidResponse)
            throw new ExternalServiceException(
                $"Elasticsearch upsert failed: {response.ElasticsearchServerError?.Error?.Reason ??
                                                response.DebugInformation}");

    }

    public async Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var response = await esClient.DeleteAsync<ProductSearchDocument>(
            _index,
            new Id(productId.ToString()),
            _ => { },
            cancellationToken);

        if (!response.IsValidResponse && response.Result is not Result.NotFound)
        {
            throw new ExternalServiceException(
                $"Elasticsearch delete failed: {response.ElasticsearchServerError?.Error?.Reason ??
                                                response.DebugInformation}");
        }
    }
}
