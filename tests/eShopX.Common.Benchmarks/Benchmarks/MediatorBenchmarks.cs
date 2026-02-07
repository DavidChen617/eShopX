using BenchmarkDotNet.Attributes;

using eShopX.Common.Mediator;
using eShopX.Common.Mediator.Behaviors;
using eShopX.Common.Validation;

using Microsoft.Extensions.DependencyInjection;

namespace eShopX.Common.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class MediatorBenchmarks
{
    private ISender _senderNoBehaviors = null!;
    private ISender _senderWithValidation = null!;
    private Ping _request = null!;

    [GlobalSetup]
    public void Setup()
    {
        _request = new Ping("ping");

        var servicesNoBehaviors = new ServiceCollection();
        servicesNoBehaviors.AddScoped<IRequestHandler<Ping, Pong>, PingHandler>();
        servicesNoBehaviors.AddScoped<ISender, Sender>();
        _senderNoBehaviors = servicesNoBehaviors.BuildServiceProvider()
            .GetRequiredService<ISender>();

        var servicesWithValidation = new ServiceCollection();
        servicesWithValidation.AddScoped<IRequestHandler<Ping, Pong>, PingHandler>();
        servicesWithValidation.AddValidators(typeof(PingValidator).Assembly);
        servicesWithValidation.AddValidationBehavior();
        servicesWithValidation.AddScoped<ISender, Sender>();
        _senderWithValidation = servicesWithValidation.BuildServiceProvider()
            .GetRequiredService<ISender>();
    }

    [Benchmark(Baseline = true)]
    public Task<Pong> Send_NoBehaviors()
        => _senderNoBehaviors.Send(_request);

    [Benchmark]
    public Task<Pong> Send_WithValidation()
        => _senderWithValidation.Send(_request);

    public sealed record Ping(string Message) : IRequest<Pong>;
    public sealed record Pong(string Message);

    public sealed class PingHandler : IRequestHandler<Ping, Pong>
    {
        public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
            => Task.FromResult(new Pong(request.Message + ":handled"));
    }

    public sealed class PingValidator : AbstractValidator<Ping>
    {
        public PingValidator()
        {
            RuleFor(x => x.Message).NotEmpty();
        }
    }
}
