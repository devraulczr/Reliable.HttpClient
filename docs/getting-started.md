# Quick Start Guide

Get up and running with HttpClient.Resilience in minutes.

## Installation

```bash
dotnet add package HttpClient.Resilience
```

## Basic Setup

### Default Configuration (Recommended)

```csharp
builder.Services.AddHttpClient("myapi", c =>
{
    c.BaseAddress = new Uri("https://api.example.com");
})
.AddResilience(); // Uses built-in defaults - no configuration needed!
```

### Custom Configuration

```csharp
builder.Services.AddHttpClient<WeatherApiClient>()
    .AddResilience(options =>
    {
        // Only configure what you need to change
        options.Retry.MaxRetries = 5; // Default: 3
        options.CircuitBreaker.FailuresBeforeOpen = 10; // Default: 5
        // All other settings use sensible defaults
    });
```

### Using the Client

```csharp
using System.Text.Json;

public class WeatherService
{
    private readonly HttpClient _httpClient;

    public WeatherService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("myapi");
    }

    public async Task<WeatherData> GetWeatherAsync(string city)
    {
        var response = await _httpClient.GetAsync($"/weather?city={city}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WeatherData>();
    }
}
```

## What You Get Out of the Box

- **Retry Policy**: 3 attempts with exponential backoff + jitter
- **Circuit Breaker**: Opens after 5 consecutive failures, stays open for 1 minute
- **Smart Error Handling**: Retries on 5xx, 408, 429, and network errors
- **Production Ready**: Used by companies in production environments

## Next Steps

- [Learn about configuration options](configuration.md)
- [Explore advanced usage patterns](advanced-usage.md)
- [See real-world examples](examples/common-scenarios.md)
