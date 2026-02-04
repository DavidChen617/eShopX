using eShopX.Common.Responses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace eShopX.Common.Exceptions.Handlers;

public sealed class ForbiddenExceptionHandler(ILogger<ForbiddenExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ForbiddenException forbidden)
        {
            return false;
        }

        logger.LogWarning("Forbidden: {Message}", forbidden.Message);

        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        await httpContext.Response.WriteAsJsonAsync(
            ApiResponse.Error(StatusCodes.Status403Forbidden, forbidden.Message),
            cancellationToken);

        return true;
    }
}
