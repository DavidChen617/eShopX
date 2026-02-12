using System.Text;
using Microsoft.Extensions.Logging;

namespace eShopX.Common.Logging;

public sealed class CustomLogger(
    string category,
    Func<CustomLoggerOptions> getOptions,
    ILogDispatcher dispatcher,
    IExternalScopeProvider? scopeProvider,
    IScopeIdAccessor? scopeIdAccessor)
    : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return scopeProvider?.Push(state) ?? NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        var options = getOptions();
        return logLevel != LogLevel.None && logLevel >= options.MinLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,                                                            
        Func<TState, Exception?, string>? formatter)
    {
        if (!IsEnabled(logLevel) || formatter is null)
        {
            return;
        }

        var message = formatter(state, exception);

        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        StringBuilder sb = new();
        sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        sb.Append(" [").Append(logLevel).Append("] ");
        if (getOptions().IncludeThreadId)
        {
            sb.Append("[T:").Append(Environment.CurrentManagedThreadId).Append("] ");
        }

        sb.Append(" [");
        sb.Append(category).Append(": ").Append(message);

        if (exception is not null)
        {
            sb.Append(" | ").Append(exception);
        }

        if (getOptions().IncludeScopes)
        {
            scopeProvider?.ForEachScope((scope, b) =>
            {
                b.Append(" => ").Append(scope);
            }, sb);
        }

        var line = sb.ToString();
        var entry = new LogEntry(line, logLevel, GetColor(logLevel), scopeIdAccessor?.ScopeId);
        dispatcher.TryEnqueue(entry);
    }

    public static ConsoleColor GetColor(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => ConsoleColor.Gray,
            LogLevel.Debug => ConsoleColor.DarkGray,
            LogLevel.Information => ConsoleColor.Green,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Critical => ConsoleColor.Magenta,
            _ => ConsoleColor.White
        };
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
