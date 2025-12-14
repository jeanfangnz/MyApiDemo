using System.Threading.Tasks;
using MyApi.Exceptions;
using Xunit;

public class ExchangeServiceTests
{
    [Fact]
    public async Task ConvertAsync_ReturnsRoundedValue_FromRoundingService()
    {
        // Mock a fake rate provider
        var rateProvider = new FakeRateProvider(rate: 2.0m);

        // Provide a different value than 20.00m to make sure round provder has been called
        var roundingProvider = new FakeRoundingProvider(roundedValue: 19.99m);

        var svc = new ExchangeService(roundingProvider, rateProvider);

        var req = new ExchangeRequest
        {
            Amount = 10m,
            InputCurrency = "USD",
            OutputCurrency = "EUR"
        };

        var result = await svc.ConvertAsync(req);

        Assert.Equal(19.99m, result);
        Assert.Equal(1, rateProvider.Calls);
        Assert.Equal(1, roundingProvider.Calls);
    }

    [Fact]
    public async Task ConvertAsync_ThrowsBusinessException_WhenSameCurrency()
    {
        // Call Exchange service with mock providers
        var svc = new ExchangeService(new FakeRoundingProvider(1.0m), new FakeRateProvider(1.0m));

        var req = new ExchangeRequest
        {
            Amount = 10m,
            InputCurrency = "USD",
            OutputCurrency = "USD"
        };

        var ex = await Assert.ThrowsAsync<BusinessException>(() => svc.ConvertAsync(req));
        Assert.Contains("InputCurrency and OutputCurrency must be different.", ex.Message);
    }
}
