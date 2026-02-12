using eShopX.Common.Extensions;
using eShopX.Common.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace eShopX.Common.Proxy;

public class LoggingProxy<T> : DispatchProxyBase<T> where T : class
{
    protected async override void OnBefore(ProxyContext context)
    {
        var http = Services.GetService<IHttpContextAccessor>()?.HttpContext;
        var scopeId = Services.GetService<IScopeIdAccessor>()?.ScopeId;
        if (http == null)
        {
            WriteLog("before", context, scopeId);
            return;
        }

        http.Request.EnableBuffering();
        string? body = null;
        using (var reader = new StreamReader(http.Request.Body, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            http.Request.Body.Position = 0;
        }

        var payload = new
        {
            Request = new
            {
                http.Request.Method,
                Path = http.Request.Path.Value,
                http.Request.Query,
                Headers = http.Request.Headers.ToDictionary(h => h.Key, h =>
                    h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
                        ? "***"
                        : h.Value.ToString()),
                Cookies = http.Request.Cookies.ToDictionary(c => c.Key, c => "***"),
                Body = body
            },
            Connection = new
            {
                RemoteIpAddress = http.Connection.RemoteIpAddress?.ToString(),
                LocalIpAddress = http.Connection.LocalIpAddress?.ToString()
            },
            http.TraceIdentifier,
            ScopeId = scopeId
        };

        Write(payload);
    }

    protected override void OnAfter(ProxyContext context)
    {
        var scopeId = Services.GetService<IScopeIdAccessor>()?.ScopeId;
        WriteLog("after", context, scopeId);
    }

    protected override void OnError(ProxyContext context, Exception exception)
    {
        var scopeId = Services.GetService<IScopeIdAccessor>()?.ScopeId;
    }

    private void WriteLog(string stage, ProxyContext context, string? scopeId, Exception? exception = null)
    {
        var safeArgs = context.Args.Select(SafeArg).ToArray();
        var payload = new
        {
            Stage = stage,
            Method = context.Method.Name,
            DeclaringType = context.Method.DeclaringType?.FullName,
            ScopeId = scopeId,
            Args = safeArgs,
            Result = context.Result,
            Exception = exception?.Message
        };

        Write(payload, stage, context.Method.Name);
    }

    private void Write(object payload, string? stage = null, string? method = null)
    {
        var logger = Services.GetService<ILogger<LoggingProxy<T>>>();
        try
        {
            var json = payload.ToJson();
            if (logger != null)
            {
                logger.LogInformation("{Payload}", json);
            }
            else
            {
                Console.WriteLine(json);
            }
        }
        catch (Exception ex)
        {
            var fallback = $"{{\"stage\":\"{stage}\",\"method\":\"{method}\",\"error\":\"{ex.Message}\"}}";
            if (logger != null)
            {
                logger.LogError("{Payload}", fallback);
            }
            else
            {
                Console.WriteLine(fallback);
            }
        }
    }

    private static object? SafeArg(object? arg)
    {
        if (arg == null) return null;
        if (arg is CancellationToken token)
        {
            return new
            {
                Type = nameof(CancellationToken),
                token.CanBeCanceled,
                token.IsCancellationRequested
            };
        }

        if (arg is Stream stream)
        {
            return new
            {
                Type = stream.GetType().Name,
                Length = stream.CanSeek ? (long?)stream.Length : null
            };
        }

        var type = arg.GetType();
        if (type.IsPrimitive || arg is string || arg is Guid || arg is DateTime || arg is decimal)
        {
            return arg;
        }

        try
        {
            var json = arg.ToJson();
            return new { Type = type.FullName, Json = json };
        }
        catch
        {
            return new { Type = type.FullName, Value = arg.ToString() };
        }
    }
}
