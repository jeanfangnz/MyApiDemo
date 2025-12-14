using System.ComponentModel.DataAnnotations;

public interface IExchangeRequest
{
    decimal Amount { get; set; }
    string InputCurrency { get; set; }
    string OutputCurrency { get; set; }
    public decimal Value { get; set; }
}

public class ExchangeRequest : IExchangeRequest
{
    // Small positive lower bound to avoid zero/negative amounts.
    [Range(0.0000001, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string InputCurrency { get; set; } = "";
    
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string OutputCurrency { get; set; } = "";
    
    public decimal Value { get; set; }
}
