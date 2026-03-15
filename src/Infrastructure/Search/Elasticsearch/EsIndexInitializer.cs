using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using eShopX.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Search.Elasticsearch;

public class EsIndexInitializer(
    ElasticsearchClient esClient,
    IOptions<ElasticsearchOptions> options,
    IProductSearchIndexService indexService,
    ILogger<EsIndexInitializer> logger)
{
    private const int EmbeddingDims = 1024;

    public async Task EnsureIndexAsync(CancellationToken ct = default)
    {
        var index = options.Value.IndexName;

        var exists = await esClient.Indices.ExistsAsync(index, ct);
        if (exists.Exists)
        {
            await PutMappingAsync(index, ct);
            return;
        }

        var create = await esClient.Indices.CreateAsync<ProductSearchDocument>(index, x => x
            .Mappings(m => m
                .Properties(p => p
                    .Keyword(d => d.ProductId)
                    .Keyword(d => d.CategoryId)
                    .Text(d => d.Name)
                    .Text(d => d.Description)
                    .FloatNumber(d => d.Price)
                    .IntegerNumber(d => d.StockQuantity)
                    .Boolean(d => d.IsActive)
                    .Date(d => d.CreatedAt)
                    .DenseVector(d => d.Embedding, dv => dv
                        .Dims(EmbeddingDims)
                        .ElementType(DenseVectorElementType.Float)
                        .Index(true)
                        .Similarity(DenseVectorSimilarity.Cosine))
                )), ct);

        if (!create.IsValidResponse)
        {
            logger.LogError("Failed to create ES index {Index}: {Error}", index,
                create.ElasticsearchServerError?.Error?.Reason);
            return;
        }

        logger.LogInformation("Created ES index {Index}, starting full reindex", index);
        var result = await indexService.ReindexAsync(cancellationToken: ct);
        logger.LogInformation("Reindex complete: total={Total} indexed={Indexed} failed={Failed}",
            result.TotalCount, result.Indexed, result.Failed);
    }

    private async Task PutMappingAsync(string index, CancellationToken ct)
    {
        var put = await esClient.Indices.PutMappingAsync<ProductSearchDocument>(x => x
            .Indices(index)
            .Properties(p => p
                .DenseVector(d => d.Embedding, dv => dv
                    .Dims(EmbeddingDims)
                    .ElementType(DenseVectorElementType.Float)
                    .Index(true)
                    .Similarity(DenseVectorSimilarity.Cosine))), ct);

        if (!put.IsValidResponse)
            logger.LogWarning("Could not update mapping for index {Index}: {Error}", index,
                put.ElasticsearchServerError?.Error?.Reason);
    }
}
