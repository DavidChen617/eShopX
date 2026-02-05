using BenchmarkDotNet.Attributes;
using eShopX.Common.Validation;

namespace eShopX.Common.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class ValidationBenchmarks
{
    private SampleValidator _validator = null!;
    private Sample _valid = null!;
    private Sample _invalid = null!;

    [GlobalSetup]
    public void Setup()
    {
        _validator = new SampleValidator();
        _valid = new Sample
        {
            Name = "ok",
            Email = "a@b.com",
            Age = 30,
            ExpectedAge = 30,
            Rating = 4,
            Token = "ok"
        };
        _invalid = new Sample
        {
            Name = "",
            Email = "bad@",
            Age = 0,
            ExpectedAge = 1,
            Rating = 10,
            Token = "bad"
        };
    }

    [Benchmark]
    public bool Validate_Sync_Valid()
        => _validator.Validate(_valid).IsValid;

    [Benchmark]
    public bool Validate_Sync_Invalid()
        => _validator.Validate(_invalid).IsValid;

    [Benchmark]
    public async Task<bool> Validate_Async_Valid()
        => (await _validator.ValidateAsync(_valid)).IsValid;

    private sealed class Sample
    {
        public string? Name { get; init; }
        public string? Email { get; init; }
        public int Age { get; init; }
        public int ExpectedAge { get; init; }
        public int Rating { get; init; }
        public string? Token { get; init; }
    }

    private sealed class SampleValidator : AbstractValidator<Sample>
    {
        public SampleValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MinLength(2);
            RuleFor(x => x.Email).Email();
            RuleFor(x => x.Age).GreaterThan(0).Equal(x => x.ExpectedAge);
            RuleFor(x => x.Rating).InclusiveBetween(1, 5);
            RuleFor(x => x.Token).MustAsync((token, ct) => Task.FromResult(token == "ok"));
        }
    }
}
