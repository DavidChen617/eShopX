namespace eShopX.Common.Logging.Sinks;

public interface ILogSink
{
    void Emit(LogEntry logEntry);
}
