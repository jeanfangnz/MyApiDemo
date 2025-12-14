using System.Net;
using System.Text;

public class LiveExchangeRateProviderTests
{
    [Fact]
    public async Task GetRateAsync_ReturnsRate_WhenCurrencyExists()
    {
        var json = """
        {
          "result": "success",
          "base_code": "USD",
          "rates": {
            "AUD": 1.55
          }
        }
        """;

        var handler = new FakeHttpMessageHandler(req =>
        {
            Assert.EndsWith("latest/USD", req.RequestUri!.ToString());

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://open.er-api.com/v6/")
        };

        var provider = new LiveExchangeRateProvider(httpClient);

        var rate = await provider.GetRateAsync("USD", "AUD");
        Assert.Equal(1.55m, rate);
    }

    [Fact]
    public async Task GetRateAsync_ThrowsBusinessException_WhenApiReturnsError()
    {
        var json = """
        {
            "result": "error",
            "error-type": "unsupported-code"
        }
        """;

        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            { 
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://open.er-api.com/v6/")
        };

        var provider = new LiveExchangeRateProvider(httpClient);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.GetRateAsync("XXX", "USD"));
        Assert.Contains("Rate not found", ex.Message);
    }

}
