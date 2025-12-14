public interface IExchangeRateProvider
{
    Task<decimal> GetRateAsync(string baseCcy, string quoteCcy, CancellationToken ct = default);
}
