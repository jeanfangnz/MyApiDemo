public sealed class FakeRoundingProvider : IRoundingProvider
{
    private readonly decimal _roundedValue;

    public int Calls { get; private set; }

    public FakeRoundingProvider(decimal roundedValue)
    {
        _roundedValue = roundedValue;
    }

    public decimal Round(string currency, decimal value)
    {
        Calls++;
        return _roundedValue;
    }
}
