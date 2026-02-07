using BenchmarkDotNet.Attributes;

using eShopX.Common.Mapping;

namespace eShopX.Common.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class MappingBenchmarks
{
    private IMapper _mapper = null!;
    private Source _single = null!;
    private List<Source> _batch = null!;

    [Params(1, 100, 1000)]
    public int BatchSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var config = new MapperConfiguration([new MappingProfile()]);
        _mapper = new Mapper(config);
        _single = new Source { First = "A", Last = "B", Age = 20 };
        _batch = Enumerable.Range(0, BatchSize)
            .Select(i => new Source { First = "F" + i, Last = "L" + i, Age = i })
            .ToList();
    }

    [Benchmark]
    public Dest MapSingle()
        => _mapper.Map<Source, Dest>(_single);

    [Benchmark]
    public int MapBatch()
    {
        var total = 0;
        foreach (var item in _batch)
        {
            total += _mapper.Map<Source, Dest>(item).Age;
        }

        return total;
    }

    public sealed class Source
    {
        public string? First { get; init; }
        public string? Last { get; init; }
        public int Age { get; init; }
    }

    public sealed class Dest
    {
        public string? FullName { get; set; }
        public int Age { get; set; }
    }

    public sealed class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Source, Dest>()
                .ForMember(d => d.FullName, s => s.First + " " + s.Last);
        }
    }
}
