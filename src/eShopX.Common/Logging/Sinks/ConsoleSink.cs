namespace eShopX.Common.Logging.Sinks;

public sealed class ConsoleSink : ILogSink
{
    public void Emit(LogEntry entry)
    {
        if (entry.Color.HasValue)
        {
            var original = Console.ForegroundColor;
            Console.ForegroundColor = entry.Color.Value;
            Console.WriteLine(entry.Message);
            Console.ForegroundColor = original;
        }
        else
        {
            Console.WriteLine(entry.Message);
        }
    }
}
