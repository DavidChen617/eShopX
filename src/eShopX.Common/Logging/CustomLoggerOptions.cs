using System.Threading.Channels;

using Microsoft.Extensions.Logging;

namespace eShopX.Common.Logging;

public class CustomLoggerOptions
{
    public LogLevel MinLevel { get; set; } = LogLevel.Information;
    public bool EnableConsole { get; set; } = true;
    public bool EnableFile { get; set; } = true;
    public bool EnableDb { get; set; } = false;
    public string LogDirectory { get; set; } = "Logs";
    public string FilePrefix { get; set; } = "app-";
    public bool IncludeScopes { get; set; } = true;
    public int ChannelCapacity { get; set; } = 1024;
    public BoundedChannelFullMode ChannelFullMode { get; set; } = BoundedChannelFullMode.DropOldest;
    public bool IncludeThreadId { get; set; } = false;
    public bool IncludeSinkThreadId { get; set; } = false;
}