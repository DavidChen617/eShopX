using BenchmarkDotNet.Attributes;
using eShopX.Common.Proxy;
using Microsoft.Extensions.DependencyInjection;

namespace eShopX.Common.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class ProxyBenchmarks
{
    private ICalc _direct = null!;
    private ICalc _proxy = null!;

    [GlobalSetup]
    public void Setup()
    {
        _direct = new Calc();
        var services = new ServiceCollection().BuildServiceProvider();
        _proxy = DispatchProxyFactory.Create<ICalc, NoopProxy>(new Calc(), services);
    }

    [Benchmark(Baseline = true)]
    public int Direct_Add()
        => _direct.Add(3, 5);

    [Benchmark]
    public int Proxy_Add()
        => _proxy.Add(3, 5);

    [Benchmark]
    public Task<int> Proxy_AddAsync()
        => _proxy.AddAsync(3, 5);

    [Benchmark]
    public ValueTask<int> Proxy_AddValueAsync()
        => _proxy.AddValueAsync(3, 5);

    private interface ICalc
    {
        int Add(int a, int b);
        Task<int> AddAsync(int a, int b);
        ValueTask<int> AddValueAsync(int a, int b);
    }

    private sealed class Calc : ICalc
    {
        public int Add(int a, int b) => a + b;
        public Task<int> AddAsync(int a, int b) => Task.FromResult(a + b);
        public ValueTask<int> AddValueAsync(int a, int b) => ValueTask.FromResult(a + b);
    }

    private sealed class NoopProxy : DispatchProxyBase<ICalc>
    {
    }
}
