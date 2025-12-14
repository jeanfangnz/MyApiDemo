using System.Text.Json.Serialization;

public class ExchangeRate
{
    [JsonPropertyName("result")]
    public string Result { get; set; } = "";

    [JsonPropertyName("base_code")]
    public string BaseCode { get; set; } = "";

    [JsonPropertyName("rates")]
    public Dictionary<string, decimal> Rates { get; set; } = new();
}
