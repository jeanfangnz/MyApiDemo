public class ExchangeRateOptions
{
    public string BaseUrl { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 5;
    public string Strategy { get; set; } = "CachedLive";
    public int CacheTtlSeconds { get; set; } = 300;
    public ProvidersOptions Providers { get; set; } = new();
}

public class ProvidersOptions
{
    public string Primary { get; set; } = "";
    public string? Fallback { get; set; }
}