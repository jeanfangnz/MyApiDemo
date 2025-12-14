using Microsoft.AspNetCore.Mvc;
using MyApi.Exceptions;

[ApiController]
[Route("[controller]")]
public class ExchangeServiceController : ControllerBase
{
    private readonly IExchangeService _svc;

    public ExchangeServiceController(IExchangeService svc) => _svc = svc;

    [HttpPost]
    [Consumes("application/json")]
    [Produces("text/plain")]
    public async Task<IActionResult> Post([FromBody] ExchangeRequest request)
    {
        var result = await _svc.ConvertAsync(request);

        var data = new
        {
            amount = request.Amount,
            inputCurrency = request.InputCurrency,
            outputCurrency = request.OutputCurrency,
            value = result
        };
        return Content(data.ToString() ?? "", "text/plain");
    }
}
