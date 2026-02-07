using eShopX.Common.Responses;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace eShopX.Common.Exceptions.Handlers;

public sealed class NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException notFoundEx)
        {
            return false;
        }

        logger.LogWarning("Resource not found: {Message}", notFoundEx.Message);
        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        await httpContext.Response.WriteAsJsonAsync(
            ApiResponse.Error(StatusCodes.Status404NotFound, notFoundEx.Message),
            cancellationToken);

        return true;
    }
}
