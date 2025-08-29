# Quick Start Guide

Get up and running with Reliable.HttpClient in minutes. Choose between core resilience features or the complete resilience + caching solution.

## Installation

### Core Resilience Only

```bash
dotnet add package Reliable.HttpClient
```

### Resilience + Caching

```bash
# Install both packages
dotnet add package Reliable.HttpClient
dotnet add package Reliable.HttpClient.Caching
```

## Basic Setup

### Default Configuration (Core Resilience)

```csharp
builder.Services.AddHttpClient("myapi", c =>
{
    c.BaseAddress = new Uri("https://api.example.com");
})
.AddResilience(); // Uses built-in defaults - no configuration needed!
```

### Adding Caching to Resilience

```csharp
// Add memory cache service
services.AddMemoryCache();

// Configure HttpClient with resilience and caching
services.AddHttpClient<WeatherApiClient>(c =>
{
    c.BaseAddress = new Uri("https://api.weather.com");
})
.AddResilience()  // Retry + Circuit breaker
.AddMemoryCache<WeatherResponse>(options =>
{
    options.DefaultExpiry = TimeSpan.FromMinutes(5);
});
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

### Using the Cached Client

```csharp
public class WeatherService
{
    private readonly CachedHttpClient<WeatherResponse> _cachedClient;

    public WeatherService(CachedHttpClient<WeatherResponse> cachedClient)
    {
        _cachedClient = cachedClient;
    }

    public async Task<WeatherResponse> GetWeatherAsync(string city)
    {
        // This call will be cached automatically
        var response = await _cachedClient.GetAsync($"/weather?city={city}");
        return response;
    }
}
```

## What You Get Out of the Box

### Core Resilience Features

- **Retry Policy**: 3 attempts with exponential backoff + jitter
- **Circuit Breaker**: Opens after 5 consecutive failures, stays open for 1 minute
- **Smart Error Handling**: Retries on 5xx, 408, 429, and network errors
- **Production Ready**: Used by companies in production environments

### Caching Features (with Reliable.HttpClient.Caching)

- **Automatic Caching**: Cache HTTP responses based on configurable rules
- **Smart Cache Keys**: SHA256-based generation with collision prevention
- **Multiple Providers**: Memory cache and custom cache providers
- **Cache Management**: Manual invalidation and clearing
- **Security**: Cryptographic hashing prevents cache poisoning

## Next Steps

- [Learn about configuration options](configuration.md)
- [Explore caching features](caching.md)
- [Explore advanced usage patterns](advanced-usage.md)
- [See real-world examples](examples/common-scenarios.md)
