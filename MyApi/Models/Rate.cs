public interface IRate
{
    public string Base { get; set; }
    Dictionary<string, decimal> Rates { get; set; }
}
public class Rate: IRate
{
    public string Base { get; set; } = "";
    public Dictionary<string, decimal> Rates { get; set; } = new();
}
