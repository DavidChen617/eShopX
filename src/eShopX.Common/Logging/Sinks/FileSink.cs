using System.Text;

namespace eShopX.Common.Logging.Sinks;

public sealed class FileSink : ILogSink
{
    private readonly string _directory;
    private readonly string _filePrefix;
    private readonly Lock _lock = new();

    public FileSink(string directory, string filePrefix)
    {
        _directory = directory;
        _filePrefix = filePrefix;
        Directory.CreateDirectory(_directory);
    }

    public void Emit(LogEntry entry)
    {
        var path = Path.Combine(_directory, _filePrefix + DateTime.Now.ToString("yyyyMMdd") + ".log");
        lock (_lock)
        {
            File.AppendAllText(path, entry.Message + Environment.NewLine + Environment.NewLine, Encoding.UTF8);
        }
    }
}
