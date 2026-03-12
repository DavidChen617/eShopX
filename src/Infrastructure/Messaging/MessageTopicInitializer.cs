using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Confluent.Kafka.Admin;
namespace Infrastructure.Messaging;

public class MessageTopicInitializer(
    List<TopicSpecification> topics,
    IAdminClient  adminClient,
    ILogger<MessageTopicInitializer> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await adminClient.CreateTopicsAsync(topics);
            logger.LogInformation("Kafka topics ensured");
        }
        catch (CreateTopicsException ex)
        {
            var unexpected = ex.Results
                .Where(r => r.Error.Code != ErrorCode.TopicAlreadyExists)
                .ToList();

            if (unexpected.Count != 0)
            {
                throw;
            }

            logger.LogInformation("Kafka topics already exist");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
