using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace eShopX.Common.Logging;

public sealed class ScopeIdMiddleware(
    RequestDelegate next,
    ILogger<ScopeIdMiddleware> logger,
    IScopeIdAccessor scopeIdAccessor)
{
    private const string HeaderName = "X-Scope-Id";

    public async Task Invoke(HttpContext context)
    {
        var scopeId =
            context.Request.Headers[HeaderName].FirstOrDefault()
            ?? Activity.Current?.TraceId.ToString()
            ?? context.TraceIdentifier;

        scopeIdAccessor.ScopeId = scopeId;

        using (logger.BeginScope($"scopeId={scopeId}"))
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = scopeId;
                return Task.CompletedTask;
            });

            await next(context);
        }
    }
}

public static class ScopeIdMiddlewareExtensions
{
    public static IApplicationBuilder UseScopeId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ScopeIdMiddleware>();
    }
}
