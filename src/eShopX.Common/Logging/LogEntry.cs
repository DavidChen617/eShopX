using Microsoft.Extensions.Logging;

namespace eShopX.Common.Logging;

public readonly record struct LogEntry(
    string Message,
    LogLevel Level,
    ConsoleColor? Color);