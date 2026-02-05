using System;
using System.Threading.Tasks;

using eShopX.Common.Proxy;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace eShopX.Common.Test;

public class ProxyTests
{
    private interface ITestService
    {
        int Add(int a, int b);
        Task<int> AddAsync(int a, int b);
        Task FailAsync();
        ValueTask<int> AddValueAsync(int a, int b);
        ValueTask FailValueAsync();
        void ThrowSync();
    }

    private sealed class TestService : ITestService
    {
        public int Add(int a, int b) => a + b;
        public Task<int> AddAsync(int a, int b) => Task.FromResult(a + b);
        public Task FailAsync() => Task.FromException(new InvalidOperationException("fail"));
        public ValueTask<int> AddValueAsync(int a, int b) => ValueTask.FromResult(a + b);
        public ValueTask FailValueAsync() => ValueTask.FromException(new InvalidOperationException("vfail"));
        public void ThrowSync() => throw new InvalidOperationException("sync");
    }

    private class TestProxy : DispatchProxyBase<ITestService>
    {
        public int BeforeCount { get; private set; }
        public int AfterCount { get; private set; }
        public int ErrorCount { get; private set; }
        public ProxyContext? LastContext { get; private set; }

        protected override void OnBefore(ProxyContext context)
        {
            BeforeCount++;
            LastContext = context;
        }

        protected override void OnAfter(ProxyContext context)
        {
            AfterCount++;
            LastContext = context;
        }

        protected override void OnError(ProxyContext context, Exception exception)
        {
            ErrorCount++;
            LastContext = context;
        }
    }

    [Fact]
    public async Task DispatchProxyBase_Intercepts_AllReturnKinds()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var proxy = DispatchProxyFactory.Create<ITestService, TestProxy>(new TestService(), services);
        var typedProxy = Assert.IsAssignableFrom<TestProxy>(proxy);

        Assert.Equal(3, proxy.Add(1, 2));
        Assert.Equal(3, await proxy.AddAsync(1, 2));
        Assert.Equal(3, await proxy.AddValueAsync(1, 2));

        Assert.Equal(3, typedProxy.BeforeCount);
        Assert.Equal(3, typedProxy.AfterCount);
        Assert.Equal(0, typedProxy.ErrorCount);
    }

    [Fact]
    public async Task DispatchProxyBase_OnError_ForExceptions()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var proxy = DispatchProxyFactory.Create<ITestService, TestProxy>(new TestService(), services);
        var typedProxy = Assert.IsAssignableFrom<TestProxy>(proxy);

        Assert.Throws<InvalidOperationException>(() => proxy.ThrowSync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => proxy.FailAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => proxy.FailValueAsync().AsTask());

        Assert.Equal(3, typedProxy.ErrorCount);
    }

    [Fact]
    public void DecorateWithDispatchProxy_WrapsRegisteredService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        services.DecorateWithDispatchProxy<ITestService, TestProxy>();

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ITestService>();

        Assert.IsAssignableFrom<TestProxy>(service);
        Assert.Equal(5, service.Add(2, 3));
    }

    [Fact]
    public void DecorateWithDispatchProxy_Throws_WhenNoService()
    {
        var services = new ServiceCollection();
        Assert.Throws<InvalidOperationException>(() =>
            services.DecorateWithDispatchProxy<ITestService, TestProxy>());
    }

    [Fact]
    public void DecorateWithDispatchProxy_Throws_ForInvalidTypes()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        Assert.Throws<ArgumentException>(() =>
            services.DecorateWithDispatchProxy(typeof(TestService), typeof(TestProxy)));

        Assert.Throws<ArgumentException>(() =>
            services.DecorateWithDispatchProxy(typeof(ITestService), typeof(GenericProxy<>)));
    }

    [Fact]
    public void DecorateWithDispatchProxyFromAttributes_UsesAttribute()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAttributedService, AttributedService>();

        services.DecorateWithDispatchProxyFromAttributes(new[] { typeof(IAttributedService).Assembly });

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IAttributedService>();

        Assert.IsAssignableFrom<AttributedProxy>(service);
        Assert.Equal("ok", service.Echo("ok"));
    }

    [Fact]
    public void LoggingProxy_WritesWithoutHttpContext()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<LoggingProxy<ITestService>>,
            TestLogger<LoggingProxy<ITestService>>>();

        var provider = services.BuildServiceProvider();
        var proxy = DispatchProxyFactory.Create<ITestService, LoggingProxy<ITestService>>(
            new TestService(), provider);

        var result = proxy.Add(2, 2);

        Assert.Equal(4, result);
    }

    private sealed class GenericProxy<T> : DispatchProxyBase<T> where T : class
    {
    }

    [UseDispatchProxy(typeof(AttributedProxy))]
    private interface IAttributedService : IInterceptable
    {
        string Echo(string input);
    }

    private sealed class AttributedService : IAttributedService
    {
        public string Echo(string input) => input;
    }

    private class AttributedProxy : DispatchProxyBase<IAttributedService>
    {
    }

    private sealed class TestLogger<T> : Microsoft.Extensions.Logging.ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel,
            Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            _ = formatter(state, exception);
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}