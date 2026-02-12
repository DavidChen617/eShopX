using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace eShopX.Common.Logging;

public class CustomLoggerProvider(
    IOptionsMonitor<CustomLoggerOptions> options, 
    ILogDispatcher dispatcher,
    IScopeIdAccessor? scopeIdAccessor
    ) : ILoggerProvider, ISupportExternalScope
{
    private readonly ConcurrentDictionary<string, CustomLogger> _loggers = new();
    private IExternalScopeProvider? _scopeProvider;

    public void Dispose()
    {
        _loggers.Clear();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name =>
        {
            return new CustomLogger(name, () => options.CurrentValue, dispatcher, _scopeProvider,  scopeIdAccessor);
        });
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }
}
