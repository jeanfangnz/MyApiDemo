using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

public class CachedExchangeRateProviderTests
{
    [Fact]
    public async Task GetRateAsync_HitsCache_OnSecondCall()
    {
        // Arrange
        var inner = new FakeRateProvider(rate: 1.23m);

        using var cache = new MemoryCache(new MemoryCacheOptions());

        var opt = Options.Create(new ExchangeRateOptions
        {
            CacheTtlSeconds = 300
        });

        var cached = new CachedExchangeRateProvider(inner, cache, opt);

        // Act
        var r1 = await cached.GetRateAsync("USD", "EUR");
        var r2 = await cached.GetRateAsync("USD", "EUR");

        // Assert
        Assert.Equal(1.23m, r1);
        Assert.Equal(1.23m, r2);
        Assert.Equal(1, inner.Calls); // 第二次命中缓存，不应再调用 inner
    }

    [Fact]
    public async Task GetRateAsync_ExpiresAfterTtl_AndCallsInnerAgain()
    {
        // Arrange
        var inner = new FakeRateProvider(rate: 1.23m);

        using var cache = new MemoryCache(new MemoryCacheOptions());

        var opt = Options.Create(new ExchangeRateOptions
        {
            CacheTtlSeconds = 1 // 1秒，方便测试
        });

        var cached = new CachedExchangeRateProvider(inner, cache, opt);

        // Act
        var r1 = await cached.GetRateAsync("USD", "EUR");

        await Task.Delay(TimeSpan.FromMilliseconds(1100)); // 等 TTL 过期（给一点 buffer）

        var r2 = await cached.GetRateAsync("USD", "EUR");

        // Assert
        Assert.Equal(1.23m, r1);
        Assert.Equal(1.23m, r2);
        Assert.Equal(2, inner.Calls); // 过期后应重新调用
    }
}
