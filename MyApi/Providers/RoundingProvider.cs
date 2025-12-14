using Microsoft.Extensions.Options;


public interface IRoundingProvider
{
    decimal Round(string currency, decimal value);
}

public class RoundingProvider : IRoundingProvider
{
    private readonly IOptionsMonitor<RoundingOptions> _opt;

    public RoundingProvider(IOptionsMonitor<RoundingOptions> opt)
    {
        _opt = opt;
    }

    /// Applies rounding rules based on configuration.
    public decimal Round(string currency, decimal value)
    {
        var o = _opt.CurrentValue;

        var decimals = o.PerCurrency.TryGetValue(currency, out var d)
            ? d
            : o.DefaultDecimals;

        // Midpoint rounding rule is configurable to support different financial rules
        var mode = Enum.TryParse<MidpointRounding>(o.Mode, ignoreCase: true, out var m)
            ? m
            : MidpointRounding.AwayFromZero;

        return Math.Round(value, decimals, mode);
    }
}