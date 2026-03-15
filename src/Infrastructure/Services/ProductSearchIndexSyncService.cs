using Elastic.Clients.Elasticsearch;
using eShopX.Application.Exceptions;
using eShopX.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class ProductSearchIndexSyncService(
    ElasticsearchClient esClient,
    IOptions<ElasticsearchOptions> options) : IProductSearchIndexSyncService
{
    private readonly string _index = options.Value.IndexName;

    public async Task UpsertProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        // TODO: Load product details from IProductRepository and index to Elasticsearch
        await Task.CompletedTask;
    }

    public async Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var response = await esClient.DeleteAsync<ProductSearchDocument>(
            _index,
            new Id(productId.ToString()),
            _ => { },
            cancellationToken);

        if (!response.IsValidResponse && response.Result is not Elastic.Clients.Elasticsearch.Result.NotFound)
        {
            throw new ExternalServiceException("Elasticsearch",
                response.ElasticsearchServerError?.Error?.Reason ?? response.DebugInformation);
        }
    }
}
