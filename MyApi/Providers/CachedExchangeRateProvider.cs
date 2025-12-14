using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

public class CachedExchangeRateProvider : IExchangeRateProvider
{
    private readonly IExchangeRateProvider _inner;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _ttl;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

    public CachedExchangeRateProvider(
        IExchangeRateProvider inner,
        IMemoryCache cache,
        IOptions<ExchangeRateOptions> opt)
    {
        _inner = inner;
        _cache = cache;
        // Cache TTL is based on the configuration in appsettings.
        _ttl = TimeSpan.FromSeconds(opt.Value.CacheTtlSeconds);
    }

    /// <summary>
    /// Decorator that adds in-memory caching on top of an exchange rate provider. 
    /// </summary>
    public async Task<decimal> GetRateAsync(string baseCcy, string quoteCcy, CancellationToken ct = default)
    {
        // Cache per currency pair (e.g. USD:AUD)
        var key = $"FX:{baseCcy}:{quoteCcy}";
        // Return cached value if possible
        if (_cache.TryGetValue(key, out decimal rate))
        {
            return rate;
        }
        // Get the per-key lock. Only requests for the SAME key will block each other.
        var gate = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try{
            // Fetch latest rate through API first
            rate = await _inner.GetRateAsync(baseCcy, quoteCcy, ct);
            // Setup cache
            _cache.Set(key, rate, _ttl);
            return rate;
        } 
        finally
        {
            gate.Release();
        }
    }
}
