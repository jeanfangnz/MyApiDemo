using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

public class CachedExchangeRateProviderTests
{
    [Fact]
    public async Task GetRateAsync_HitsCache_OnSecondCall()
    {
        // Mock a fake rate provider
        var inner = new FakeRateProvider(rate: 1.23m);

        using var cache = new MemoryCache(new MemoryCacheOptions());

        var opt = Options.Create(new ExchangeRateOptions
        {
            CacheTtlSeconds = 300
        });

        var cached = new CachedExchangeRateProvider(inner, cache, opt);

        // Access cache
        var r1 = await cached.GetRateAsync("USD", "EUR");
        var r2 = await cached.GetRateAsync("USD", "EUR");

        Assert.Equal(1.23m, r1);
        Assert.Equal(1.23m, r2);
        Assert.Equal(1, inner.Calls); // Should read cache in the second time, only call live rate provider once
    }

    [Fact]
    public async Task GetRateAsync_ExpiresAfterTtl_AndCallsInnerAgain()
    {
        // Mock a fake rate provider
        var inner = new FakeRateProvider(rate: 1.23m);

        using var cache = new MemoryCache(new MemoryCacheOptions());

        var opt = Options.Create(new ExchangeRateOptions
        {
            CacheTtlSeconds = 1 // Expire after 1s
        });

        var cached = new CachedExchangeRateProvider(inner, cache, opt);

        var r1 = await cached.GetRateAsync("USD", "EUR");

        await Task.Delay(TimeSpan.FromMilliseconds(1100)); // Wait until ttl expire

        var r2 = await cached.GetRateAsync("USD", "EUR");

        Assert.Equal(1.23m, r1);
        Assert.Equal(1.23m, r2);
        Assert.Equal(2, inner.Calls); // Should recall the live rate provider again
    }
}
