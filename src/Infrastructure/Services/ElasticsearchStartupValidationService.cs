using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ElasticsearchStartupValidationService(
    ElasticsearchClient esClient,
    ILogger<ElasticsearchStartupValidationService> logger): IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var ping = await esClient.PingAsync(cancellationToken);
            if (!ping.IsValidResponse)
            {
                logger.LogWarning("Elasticsearch ping failed: {Debug}", ping.DebugInformation);
                return;
            }

            var health = await esClient.Cluster.HealthAsync(cancellationToken: cancellationToken);
            logger.LogInformation("Elasticsearch connected. Cluster={Cluster}, Status={Status}",
                health.ClusterName, health.Status);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Elasticsearch startup validation failed.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
