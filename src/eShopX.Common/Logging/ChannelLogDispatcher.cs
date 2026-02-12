using System.Threading.Channels;
using eShopX.Common.Logging.Sinks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace eShopX.Common.Logging;

public sealed class ChannelLogDispatcher : ILogDispatcher, IDisposable
{
    private readonly Channel<LogEntry> _channel;
    private readonly IOptionsMonitor<CustomLoggerOptions> _options;
    private readonly object _sinkLock = new();
    private readonly IServiceProvider _serviceProvider;
    private IReadOnlyList<ILogSink> _sinks = Array.Empty<ILogSink>();
    private Task? _processingTask;

    public ChannelLogDispatcher(IOptionsMonitor<CustomLoggerOptions> options, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        BuildSinks(_options.CurrentValue);
        _options.OnChange(BuildSinks);

        var capacity = Math.Max(1, _options.CurrentValue.ChannelCapacity);
        var channelOptions = new BoundedChannelOptions(capacity)
        {
            SingleReader = true, SingleWriter = false, FullMode = _options.CurrentValue.ChannelFullMode
        };

        _channel = Channel.CreateBounded<LogEntry>(channelOptions);
    }

    public Task Completion => _processingTask ?? Task.CompletedTask;

    public bool TryEnqueue(LogEntry entry)
    {
        return _channel.Writer.TryWrite(entry);
    }

    public ValueTask EnqueueAsync(LogEntry entry, CancellationToken cancellationToken)
    {
        return _channel.Writer.WriteAsync(entry, cancellationToken);
    }

    public Task ProcessAsync(CancellationToken cancellationToken)
    {
        if (_processingTask is not null)
        {
            return _processingTask;
        }

        _processingTask = ProcessInternalAsync(cancellationToken);
        return _processingTask;
    }

    public void Complete()
    {
        _channel.Writer.TryComplete();
    }

    public void Dispose()
    {
        Complete();
        DisposeSinks();
    }

    private async Task ProcessInternalAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var entry in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                Write(entry);
            }
        }
        catch (OperationCanceledException)
        {
            // ignore cancellation
        }
    }

    private void Write(LogEntry entry)
    {
        var options = _options.CurrentValue;
        var message = options.IncludeSinkThreadId
            ? $"[S:{Environment.CurrentManagedThreadId}] {entry.Message}"
            : entry.Message;

        var finalEntry = entry with { Message = message };

        IReadOnlyList<ILogSink> sinks;
        lock (_sinkLock)
        {
            sinks = _sinks;
        }

        foreach (var sink in sinks)
        {
            sink.Emit(finalEntry);
        }
    }

    private void BuildSinks(CustomLoggerOptions options)
    {
        lock (_sinkLock)
        {
            DisposeSinks();

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
                var dbSink = _serviceProvider.GetService<DbSink>();
                if (dbSink != null) list.Add(dbSink);
            }

            _sinks = list;
        }
    }

    private void DisposeSinks()
    {
        foreach (var sink in _sinks)
        {
            if (sink is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _sinks = Array.Empty<ILogSink>();
    }
}
