using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using eShopX.Common.Exceptions;
using eShopX.Common.Logging;
using eShopX.Common.Mediator;
using eShopX.Common.Mediator.Behaviors;
using eShopX.Common.Validation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;

namespace eShopX.Common.Test;

public class MediatorTests
{
    private sealed record Ping(string Message) : IRequest<Pong>;
    private sealed record Pong(string Message);
    private sealed record Fire(string Message) : IRequest;

    private sealed class PingHandler : IRequestHandler<Ping, Pong>
    {
        public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
            => Task.FromResult(new Pong(request.Message + ":handled"));
    }

    private sealed class FireHandler : IRequestHandler<Fire>
    {
        public Task Handle(Fire request, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class PingValidator : AbstractValidator<Ping>
    {
        public PingValidator()
        {
            RuleFor(x => x.Message).NotEmpty();
        }
    }

    private sealed class RecordingBehavior : IPipelineBehavior<Ping, Pong>
    {
        private readonly List<string> _events;
        private readonly string _name;

        public RecordingBehavior(List<string> events, string name)
        {
            _events = events;
            _name = name;
        }

        public async Task<Pong> Handle(Ping request, RequestHandlerDelegate<Pong> next, CancellationToken cancellationToken)
        {
            _events.Add(_name + ":before");
            var response = await next();
            _events.Add(_name + ":after");
            return response;
        }
    }

    [Fact]
    public async Task Sender_RunsBehaviors_InRegistrationOrder()
    {
        var events = new List<string>();
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<Ping, Pong>, PingHandler>();
        services.AddScoped<IPipelineBehavior<Ping, Pong>>(_ => new RecordingBehavior(events, "A"));
        services.AddScoped<IPipelineBehavior<Ping, Pong>>(_ => new RecordingBehavior(events, "B"));
        services.AddScoped<ISender, Sender>();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var result = await sender.Send(new Ping("x"));

        Assert.Equal("x:handled", result.Message);
        Assert.Equal(new[] { "A:before", "B:before", "B:after", "A:after" }, events);
    }

    [Fact]
    public async Task Sender_SendsVoidRequest()
    {
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<Fire>, FireHandler>();
        services.AddScoped<ISender, Sender>();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        await sender.Send(new Fire("x"));
    }

    [Fact]
    public async Task ValidationBehavior_Throws_WhenFailures()
    {
        var behavior = new ValidationBehavior<Ping, Pong>([new PingValidator()]);

        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(new Ping(""), () => Task.FromResult(new Pong("ok")), CancellationToken.None));
    }

    [Fact]
    public async Task ValidationBehavior_Passes_WhenNoValidators()
    {
        var behavior = new ValidationBehavior<Ping, Pong>(Array.Empty<IValidator<Ping>>());

        var result = await behavior.Handle(new Ping("x"), () => Task.FromResult(new Pong("ok")), CancellationToken.None);

        Assert.Equal("ok", result.Message);
    }

    [Fact]
    public async Task LoggingBehavior_LogsStartAndEnd()
    {
        var logger = new TestLogger<LoggingBehavior<Ping, Pong>>();
        var scope = new ScopeIdAccessor { ScopeId = "abc123" };
        var behavior = new LoggingBehavior<Ping, Pong>(logger, scope);

        var result = await behavior.Handle(new Ping("x"), () => Task.FromResult(new Pong("ok")), CancellationToken.None);

        Assert.Equal("ok", result.Message);
        Assert.Contains(logger.Messages, m => m.Contains("START Ping"));
        Assert.Contains(logger.Messages, m => m.Contains("END Ping"));
    }

    [Fact]
    public async Task LoggingBehavior_LogsFailureAndRethrows()
    {
        var logger = new TestLogger<LoggingBehavior<Ping, Pong>>();
        var scope = new ScopeIdAccessor { ScopeId = "abc123" };
        var behavior = new LoggingBehavior<Ping, Pong>(logger, scope);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(new Ping("x"), () => throw new InvalidOperationException("boom"), CancellationToken.None));

        Assert.Contains(logger.Messages, m => m.Contains("FAIL Ping"));
    }

    [Fact]
    public void MediatorExtensions_RegisterHandlers_AndValidators()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IScopeIdAccessor, ScopeIdAccessor>();
        services.AddMediator(typeof(PingHandler).Assembly);
        services.AddValidators(typeof(PingValidator).Assembly);
        services.AddValidationBehavior();
        services.AddLoggingBehavior();

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<ISender>());
        Assert.NotNull(provider.GetService<IRequestHandler<Ping, Pong>>());
        Assert.NotNull(provider.GetService<IValidator<Ping>>());
        Assert.NotNull(provider.GetService<IPipelineBehavior<Ping, Pong>>());
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}