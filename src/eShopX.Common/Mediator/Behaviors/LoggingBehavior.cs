using System.Diagnostics;
using System.Text.Json;
using eShopX.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace eShopX.Common.Mediator.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    eShopX.Common.Logging.IScopeIdAccessor scopeIdAccessor)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new StreamJsonConverter() }
    };

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var scopeId = scopeIdAccessor.ScopeId ?? Guid.NewGuid().ToString("N")[..8];

        logger.LogInformation(
            "[{ScopeId}] START {RequestName} {@Payload}",
            scopeId,
            requestName,
            request.ToJson(JsonOptions));

        var sw = Stopwatch.StartNew();

        try
        {
            var response = await next();
            sw.Stop();

            logger.LogInformation(
                "[{ScopeId}] END {RequestName} completed in {ElapsedMs}ms",
                scopeId,
                requestName,
                sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();

            logger.LogError(
                ex,
                "[{ScopeId}] FAIL {RequestName} failed after {ElapsedMs}ms: {ErrorMessage}",
                scopeId,
                requestName,
                sw.ElapsedMilliseconds,
                ex.Message);

            throw;
        }
    }
}
