using System.Text;
using eShopX.Common.Logging.Sinks;
using Microsoft.Extensions.Logging;

namespace eShopX.Common.Logging;

// TODO
//
//     - 效能：目前是同步寫入；高並發時 Console/File 可能阻塞，需改成非同步批次或背景佇列寫入
//     - 效能：FileSink 每筆都 AppendAllText，需改成持久 Stream 或批次 flush
//     - 效能：多 sink 逐一同步寫入，需加佇列與背壓策略
//     - DB：DbSink.Emit 尚未實作（連線、批次、重試、失敗落地）
//     - DB：尚未定義表結構與索引（如時間、等級、Category、TraceId）
public sealed class CustomLogger(
    string category,
    Func<CustomLoggerOptions> getOptions,
    IReadOnlyList<ILogSink> sinks,
    IExternalScopeProvider? scopeProvider)
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
        sb.AppendLine();
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

        foreach (var sink in sinks)
        {
            if (sink is ConsoleSink consoleSink)
            {
                consoleSink.Emit(line, GetColor(logLevel));
            }
            else
            {
                sink.Emit(line);
            }
        }
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
