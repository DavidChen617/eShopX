using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace eShopX.Common.Logging;

public static class CustomLoggerExtension
{
    extension(ILoggingBuilder builder)
    {
        public ILoggingBuilder AddCustomLogger(Action<CustomLoggerOptions>? configure = null)
        {
            builder.Services.AddOptions<CustomLoggerOptions>();
            if (configure != null)
            {
                builder.Services.Configure(configure);
            }

            builder.Services.AddSingleton<ILogDispatcher, ChannelLogDispatcher>();
            builder.Services.AddHostedService<LogBackgroundService>();
            builder.Services.AddSingleton<ILoggerProvider, CustomLoggerProvider>();
            return builder;
        }
    }
}
