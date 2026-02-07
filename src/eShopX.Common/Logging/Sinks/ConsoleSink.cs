namespace eShopX.Common.Logging.Sinks;

public sealed class ConsoleSink : ILogSink
{
    public void Emit(string message)
    {
        Console.WriteLine(message);
    }

    public void Emit(string message, ConsoleColor color)
    {
        var original = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = original;
    }
}
