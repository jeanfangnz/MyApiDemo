namespace MyApi.Exceptions;

public interface IExchangeService
{
    Task<decimal> ConvertAsync(ExchangeRequest req, CancellationToken ct = default);
}

public class ExchangeService(IRoundingProvider rounding, IExchangeRateProvider rateProvider) : IExchangeService
{
    private readonly IRoundingProvider _rounding = rounding;
    private readonly IExchangeRateProvider _rateProvider = rateProvider;

    /// <summary>
    /// Handles currency conversion business logic: validates input, fetch exchange rate, applies rounding rules and return converted amount
    /// </summary>
    public async Task<decimal> ConvertAsync(ExchangeRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.InputCurrency) || string.IsNullOrWhiteSpace(req.OutputCurrency)) 
        {
            throw new ArgumentException("InputCurrency or OutputCurrency is required");
        }

        if (req.InputCurrency == req.OutputCurrency)
        {
            throw new BusinessException("InputCurrency and OutputCurrency must be different.");
        }

        // Check if the currency can be supported, additional currecies can be added if modify the conversion in Models/Currency.cs
        if (!Currency.IsValid(req.InputCurrency))
        {
            throw new BusinessException("Unsupported InputCurrency.");
        }
        
        if (!Currency.IsValid(req.OutputCurrency))
        {
            throw new BusinessException("Unsupported OutputCurrency.");
        }
        
        // Only positive amounts are supported, negative values imply refund or accounting semantics should be handled differently
        if (req.Amount < 0) {
            throw new ArgumentException("Amount must be >= 0");
        }

        var rate = await _rateProvider.GetRateAsync(req.InputCurrency, req.OutputCurrency, ct);

        // Apply rounding service based on the options setup in appsettings
        return _rounding.Round(req.OutputCurrency, req.Amount * rate);
    }
}
