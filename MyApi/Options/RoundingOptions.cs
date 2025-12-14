public class RoundingOptions
{
    public int DefaultDecimals { get; set; } = 2;
    public string Mode { get; set; } = "AwayFromZero";
    public Dictionary<string, int> PerCurrency { get; set; } = new();
}