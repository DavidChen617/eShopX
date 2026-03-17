using Elastic.Clients.Elasticsearch;
using eShopX.Application.Exceptions;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Products;
using Infrastructure.Data;
using Infrastructure.Search.Embedding;
using Microsoft.Extensions.Options;

namespace Infrastructure.Search.Elasticsearch;

public class ProductSearchIndexSynchronizer(
    ElasticsearchClient esClient,
    IOptions<ElasticsearchOptions> options,
    IProductRepository productRepository,
    IEmbeddingClient embeddingClient,
    EShopContext db) : IProductSearchIndexSynchronizer
{
    private readonly string _index = options.Value.IndexName;

    public async Task UpsertProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken);
        if (product is null)
            return;

        var categoryName = await db.Categories
            .Where(c => c.Id == product.CategoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(cancellationToken);

        var tagIds = product.Tags.Select(t => t.TagId).ToList();
        var tagNames = tagIds.Count > 0
            ? await db.Tags
                .Where(t => tagIds.Contains(t.Id))
                .Select(t => t.Name)
                .ToListAsync(cancellationToken)
            : [];

        var colors = product.Variants.Select(v => v.Color).Distinct().ToList();
        var allSkus = product.Variants.SelectMany(v => v.Skus).ToList();
        var primaryImage = product.Variants
            .SelectMany(v => v.Images)
            .FirstOrDefault(i => i.IsPrimary)
            ?? product.Variants.SelectMany(v => v.Images).FirstOrDefault();

        var embeddingText = BuildEmbeddingText(
            product.Name,
            product.Description,
            categoryName,
            colors,
            tagNames,
            product.Audience);

        var embedding = await embeddingClient.GetEmbeddingAsync(embeddingText, cancellationToken);

        var doc = new ProductSearchDocument
        {
            ProductId       = product.Id,
            CategoryId      = product.CategoryId,
            Name            = product.Name,
            Description     = product.Description ?? string.Empty,
            Price           = allSkus.Count > 0 ? allSkus.Min(s => s.Price.Amount) : 0,
            StockQuantity   = allSkus.Sum(s => s.StockQuantity),
            IsActive        = product.IsActive,
            Audience        = product.Audience?.ToString(),
            PrimaryImageUrl = primaryImage?.Url,
            CreatedAt       = product.CreatedAt,
            Embedding       = embedding
        };

        var response = await esClient.IndexAsync(doc, i => i
            .Index(_index)
            .Id(productId.ToString()), cancellationToken);

        if (!response.IsValidResponse)
            throw new ExternalServiceException("Elasticsearch",
                response.ElasticsearchServerError?.Error?.Reason ?? response.DebugInformation);
    }

    public async Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var response = await esClient.DeleteAsync<ProductSearchDocument>(
            _index,
            new Id(productId.ToString()),
            _ => { },
            cancellationToken);

        if (!response.IsValidResponse && response.Result is not Result.NotFound)
            throw new ExternalServiceException("Elasticsearch",
                response.ElasticsearchServerError?.Error?.Reason ?? response.DebugInformation);
    }

    private static string BuildEmbeddingText(
        string name,
        string? description,
        string? categoryName,
        IEnumerable<string> colors,
        IEnumerable<string> tagNames,
        Audience? audience)
    {
        var parts = new List<string> { name };

        if (!string.IsNullOrWhiteSpace(description))
            parts.Add(description);

        if (!string.IsNullOrWhiteSpace(categoryName))
            parts.Add(categoryName);

        var colorList = string.Join(" ", colors);
        if (!string.IsNullOrWhiteSpace(colorList))
            parts.Add(colorList);

        var tags = string.Join(" ", tagNames);
        if (!string.IsNullOrWhiteSpace(tags))
            parts.Add(tags);

        if (audience.HasValue)
            parts.Add(audience.Value == Audience.Men ? "男" : "女");

        return string.Join(". ", parts);
    }
}
