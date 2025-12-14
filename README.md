# MyApi – Simple Exchange Service API
A simple ASP.NET Core Web API for currency conversion between USD and AUD. The system uses an external exchange rate provider, applies configurable rounding rules, and uses caching and retry mechanisms to improve reliability.

---

## Features
- Currency conversion with configurable rounding rules
- External exchange rate integration with ExchangeRate API Open Access
- In-memory caching with TTL for exchange rate
- Concurrency de-duplication on cache miss
- Retry policy for transient HTTP failures
- Unit-testable project

---

## How to Run the Solution
- .NET SDK 10.0 or later
- macOS / Linux / Windows

### Runtime Dependencies
- Polly (used for retrying transient failures when calling the external exchange rate provider)

## Run the API
```bash
dotnet restore
dotnet build
dotnet run --project MyApi
```

## Run Unit Tests
```bash
dotnet test MyApi.Tests
```

# Configuration (managed via `appsettings.json`)

### Rounding Configuration
- `DefaultDecimals`: fallback decimals when currency is not configured.
- `PerCurrency`: Why PerCurrency under Rounding?
Different currencies have different decimal conventions (for example, JPY uses no decimals, USD uses 2, and KWD uses 3).
This can avoid hard-coding currency-specific rules in business logic, make it easy to add or update currencies in the future.
- `Mode`: rounding strategy (default `AwayFromZero`).

### Exchange Rate Configuration
- `BaseUrl`: external provider endpoint.
- `TimeoutSeconds`: fail-fast protection for external calls.
- `Strategy`: defines rate resolution behavior (e.g. cached + live).
- `CacheTtlSeconds`: controls cache freshness.
- `Primary / Fallback`: failover is not implemented in the current version, intended to support multiple exchange-rate providers.

# Caveats & Known Limitations
- Exchange rates are updated once per day and no SLA or guarantees on availability.
- In-memory cache is process-local and not suitable for multi-instance deployments.
- No authentication or authorization and persistence layer.

# Possible Improvements
- Replace in-memory cache with distributed cache (e.g. Redis)
- Support multiple exchange rate providers via configuration
- Add metrics and structured logging
- Add integration tests
- Add Dockerfile and CI pipeline
- Add Swagger / OpenAPI documentation for improved developer experience
