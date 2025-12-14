public class LiveExchangeRateProvider : IExchangeRateProvider
{
    private readonly HttpClient _http;

    public LiveExchangeRateProvider(HttpClient http) => _http = http;

    /// <summary>
    /// Use ExchangeRate API Open Access to fetch latest rates. No API key. Rates updated once per day.
    /// </summary>
    public async Task<decimal> GetRateAsync(string baseCcy, string quoteCcy, CancellationToken ct = default)
    {
        var rateResult = await _http.GetFromJsonAsync<ExchangeRate>(
            $"latest/{baseCcy}",
            ct);

        if (rateResult == null || !rateResult.Rates.TryGetValue(quoteCcy, out var rate)) 
        {
            throw new InvalidOperationException("Rate not found");
        }
        return rate;          
    }
}
