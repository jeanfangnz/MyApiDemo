using System.Threading.Tasks;
using MyApi.Exceptions;
using Xunit;

public class ExchangeServiceTests
{
    [Fact]
    public async Task ConvertAsync_ReturnsRoundedValue_FromRoundingService()
    {
        // Arrange
        var rateProvider = new FakeRateProvider(rate: 2.0m);

        // 假设 amount * rate = 10 * 2 = 20，但 rounding 最终返回 19.99（为了验证 rounding 被调用）
        var roundingProvider = new FakeRoundingProvider(roundedValue: 19.99m);

        var svc = new ExchangeService(roundingProvider, rateProvider);

        var req = new ExchangeRequest
        {
            Amount = 10m,
            InputCurrency = "USD",
            OutputCurrency = "EUR"
        };

        // Act
        var result = await svc.ConvertAsync(req);

        // Assert
        Assert.Equal(19.99m, result);
        Assert.Equal(1, rateProvider.Calls);
        Assert.Equal(1, roundingProvider.Calls);
    }

    [Fact]
    public async Task ConvertAsync_ThrowsBusinessException_WhenSameCurrency()
    {
        // Arrange
        var svc = new ExchangeService(new FakeRoundingProvider(1.0m), new FakeRateProvider(1.0m));

        var req = new ExchangeRequest
        {
            Amount = 10m,
            InputCurrency = "USD",
            OutputCurrency = "USD"
        };

        // Act + Assert
        var ex = await Assert.ThrowsAsync<BusinessException>(() => svc.ConvertAsync(req));
        Assert.Contains("InputCurrency and OutputCurrency must be different.", ex.Message);
    }
}
