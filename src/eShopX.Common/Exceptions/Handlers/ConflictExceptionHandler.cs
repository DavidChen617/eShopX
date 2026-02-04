using eShopX.Common.Responses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace eShopX.Common.Exceptions.Handlers;

public sealed class ConflictExceptionHandler(ILogger<ConflictExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ConflictException conflict)
        {
            return false;
        }

        logger.LogWarning("Conflict occurred: {Message}", conflict.Message);

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        await httpContext.Response.WriteAsJsonAsync(
            ApiResponse.Error(StatusCodes.Status409Conflict, conflict.Message),
            cancellationToken);

        return true;
    }
}
