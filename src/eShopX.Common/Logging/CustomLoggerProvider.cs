using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace eShopX.Common.Logging;

public class CustomLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ConcurrentDictionary<string, CustomLogger> _loggers = new();
    private readonly IOptionsMonitor<CustomLoggerOptions> _options;
    private readonly ILogDispatcher _dispatcher;
    private IExternalScopeProvider? _scopeProvider;

    public CustomLoggerProvider(IOptionsMonitor<CustomLoggerOptions> options, ILogDispatcher dispatcher)
    {
        _options = options;
        _dispatcher = dispatcher;
    }

    public void Dispose()
    {
        _loggers.Clear();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name =>
        {
            return new CustomLogger(name, () => _options.CurrentValue, _dispatcher, _scopeProvider);
        });
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }
}
