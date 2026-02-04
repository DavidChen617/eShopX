using System.Collections.Concurrent;
using eShopX.Common.Logging.Sinks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace eShopX.Common.Logging;

public class CustomLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ConcurrentDictionary<string, CustomLogger> _loggers = new();
    private readonly IOptionsMonitor<CustomLoggerOptions> _options;
    private IExternalScopeProvider? _scopeProvider;
    private IReadOnlyList<ILogSink> _sinks = Array.Empty<ILogSink>();

    public CustomLoggerProvider(IOptionsMonitor<CustomLoggerOptions> options)
    {
        _options = options;
        BuildSinks(_options.CurrentValue);
        _options.OnChange(BuildSinks);
    }

    public void Dispose()
    {
        _loggers.Clear();

        foreach (var sink in _sinks)
        {
            if (sink is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name =>
        {
            return new CustomLogger(name, () => _options.CurrentValue, _sinks, _scopeProvider);
        });
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    private void BuildSinks(CustomLoggerOptions options)
    {
        List<ILogSink> list = new();

        if (options.EnableConsole)
        {
            list.Add(new ConsoleSink());
        }

        if (options.EnableFile)
        {
            list.Add(new FileSink(options.LogDirectory, options.FilePrefix));
        }

        if (options.EnableDb)
        {
            list.Add(new DbSink());
        }

        _sinks = list;
    }
}
