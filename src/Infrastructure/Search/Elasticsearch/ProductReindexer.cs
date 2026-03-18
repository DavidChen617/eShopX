using Elastic.Clients.Elasticsearch;
using Domain.Aggregates.Products;
using Infrastructure.Data;
using Infrastructure.Search.Embedding;
using Microsoft.Extensions.Options;

namespace Infrastructure.Search.Elasticsearch;

public class ProductReindexer(
    IProductRepository productRepository,
    EShopContext db,
    IEmbeddingClient embeddingClient,
    ElasticsearchClient esClient,
    IOptions<ElasticsearchOptions> options) : IProductSearchIndexService
{
    private readonly string _index = options.Value.IndexName;

    public async Task<ReindexProductsResult> ReindexAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        var page = 1;
        var indexed = 0;
        var failed = 0;
        int totalCount;

        do
        {
            var (items, total) = await productRepository.GetPagedAsync(
                categoryId: null,
                audience: null,
                isActive: null,
                page: page,
                pageSize: batchSize,
                cancellationToken: cancellationToken);

            totalCount = total;

            if (items.Count == 0) break;

            // Batch load categories
            var categoryIds = items
                .Select(p => p.CategoryId)
                .Distinct()
                .ToList();

            var categoryNames = await db.Categories
                .Where(c => categoryIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

            // Batch load tags
            var productIds = items.Select(p => p.Id).ToList();
            var tagsByProduct = await db.ProductTags
                .Where(pt => productIds.Contains(pt.ProductId))
                .Join(db.Tags, pt => pt.TagId, t => t.Id, (pt, t) => new { pt.ProductId, t.Name })
                .GroupBy(x => x.ProductId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(x => x.Name).ToList(),
                    cancellationToken);

            // Build embedding texts and pre-fill docs (without embeddings yet)
            var embeddingTexts = new List<string>(items.Count);
            var docs = new List<ProductSearchDocument>(items.Count);

            foreach (var product in items)
            {
                categoryNames.TryGetValue(product.CategoryId, out var categoryName);
                tagsByProduct.TryGetValue(product.Id, out var tagNames);

                var colors = product.Variants.Select(v => v.Color).Distinct();
                var allSkus = product.Variants.SelectMany(v => v.Skus).ToList();
                var primaryImage = product.Variants
                    .SelectMany(v => v.Images)
                    .FirstOrDefault(i => i.IsPrimary)
                    ?? product.Variants.SelectMany(v => v.Images).FirstOrDefault();

                embeddingTexts.Add(BuildEmbeddingText(
                    product.Name, product.Description, categoryName,
                    colors, tagNames ?? [], product.Audience));

                docs.Add(new ProductSearchDocument
                {
                    ProductId     = product.Id,
                    CategoryId    = product.CategoryId,
                    Name          = product.Name,
                    Description   = product.Description ?? string.Empty,
                    Price         = allSkus.Count > 0 ? allSkus.Min(s => s.Price.Amount) : 0,
                    StockQuantity = allSkus.Sum(s => s.StockQuantity),
                    IsActive      = product.IsActive,
                    Audience      = product.Audience?.ToString(),
                    PrimaryImageUrl = primaryImage?.Url,
                    CreatedAt     = product.CreatedAt,
                });
            }

            // One API call for all embeddings in this batch
            float[][] embeddings;
            try
            {
                embeddings = await embeddingClient.GetEmbeddingsAsync(embeddingTexts, cancellationToken);
            }
            catch
            {
                failed += docs.Count;
                page++;
                continue;
            }

            for (var i = 0; i < docs.Count; i++)
                docs[i].Embedding = embeddings[i];

            // ES Bulk index
            var bulkResponse = await esClient.BulkAsync(b =>
            {
                foreach (var doc in docs)
                    b.Index(doc, op => op.Index(_index).Id(doc.ProductId.ToString()));
            }, cancellationToken);

            if (bulkResponse.Errors)
            {
                foreach (var item in bulkResponse.Items)
                {
                    if (item.Error is not null) failed++;
                    else indexed++;
                }
            }
            else
            {
                indexed += docs.Count;
            }

            page++;
        } while ((page - 1) * batchSize < totalCount);

        return new ReindexProductsResult(totalCount, indexed, failed);
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
