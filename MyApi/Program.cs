using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using MyApi.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddScoped<IExchangeService, ExchangeService>();
builder.Services.Configure<RoundingOptions>(
    builder.Configuration.GetSection("Rounding"));
builder.Services.Configure<ExchangeRateOptions>(
    builder.Configuration.GetSection("ExchangeRate"));
builder.Services.AddSingleton<IRoundingProvider, RoundingProvider>();
// In-memory cache used for exchange rate caching
builder.Services.AddMemoryCache();

// Register HttpClient for the live exchange rate provider with a simple retry policy.
// Retry is applied only for transient failures (network errors, 5xx, 408).
builder.Services
    .AddHttpClient<LiveExchangeRateProvider>()
    .AddPolicyHandler(
        HttpPolicyExtensions
            .HandleTransientHttpError() 
            .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(200))
    );

// Configure the LiveExchangeRateProvider HttpClient using options.
// Note: this provider is NOT exposed directly; it is wrapped by a cache decorator.
builder.Services.AddHttpClient<LiveExchangeRateProvider>((sp, client) =>
{
    var opt = sp.GetRequiredService<IOptions<ExchangeRateOptions>>().Value;
    client.BaseAddress = new Uri(opt.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(opt.TimeoutSeconds);
});

// CachedExchangeRateProvider is the only implementation exposed as IExchangeRateProvider.
// It decorates the live provider with in-memory caching and concurrency de-duplication.
builder.Services.AddSingleton<IExchangeRateProvider>(sp =>
{
    var live = sp.GetRequiredService<LiveExchangeRateProvider>();
    var cache = sp.GetRequiredService<IMemoryCache>();
    var opt = sp.GetRequiredService<IOptions<ExchangeRateOptions>>();
    return new CachedExchangeRateProvider(live, cache, opt);
});

var app = builder.Build();

// Global exception handling to map domain exceptions to HTTP responses.
// This avoids leaking implementation details and keeps controllers thin.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        context.Response.ContentType = "application/json";

        switch (exception)
        {
            case BusinessException be:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = be.Message });
                break;

            case TimeoutException:
                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                await context.Response.WriteAsJsonAsync(new { error = "Rate provider timeout" });
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { error = "Internal server error" });
                break;
        }
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRouting();
app.MapControllers();
app.UseHttpsRedirection();
app.Run();
