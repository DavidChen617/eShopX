using Microsoft.Extensions.Hosting;

namespace eShopX.Common.Logging;

public sealed class LogBackgroundService(ILogDispatcher dispatcher) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return dispatcher.ProcessAsync(CancellationToken.None);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        dispatcher.Complete();
        await dispatcher.Completion.WaitAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
