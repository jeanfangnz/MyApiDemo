using System.Threading;
using System.Threading.Tasks;

public sealed class FakeRateProvider : IExchangeRateProvider
{
    private readonly decimal _rate;

    public int Calls { get; private set; }

    public FakeRateProvider(decimal rate)
    {
        _rate = rate;
    }

    public Task<decimal> GetRateAsync(string baseCcy, string quoteCcy, CancellationToken ct = default)
    {
        Calls++;
        return Task.FromResult(_rate);
    }
}
