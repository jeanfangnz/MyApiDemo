public static class Currency
{
    private static readonly HashSet<string> Known = new()
    {
        "USD","AUD","EUR","JPY","GBP","NZD","CNY"
    };

    public static bool IsValid(string ccy) => Known.Contains(ccy.ToUpperInvariant());
}
