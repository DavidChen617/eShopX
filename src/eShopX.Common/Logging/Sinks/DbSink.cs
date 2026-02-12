using Microsoft.Extensions.DependencyInjection;

namespace eShopX.Common.Logging.Sinks;

public sealed class DbSink(IServiceProvider serviceProvider) : ILogSink
{
    public void Emit(LogEntry entry)
    {
        using var scope = serviceProvider.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<ILogDbProvider>();

        provider.Add(new ApplicationLog
        {
            ScopeId = entry.ScopeId,                                                                                                              
            Message = entry.Message,
            CreatedAt = DateTime.UtcNow
        });
    }
}

public interface ILogDbProvider
{
    void Add(ApplicationLog log);
}
