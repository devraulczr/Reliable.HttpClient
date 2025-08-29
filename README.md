# Reliable.HttpClient

## Core Package

[![NuGet Version](https://img.shields.io/nuget/v/Reliable.HttpClient)](https://www.nuget.org/packages/Reliable.HttpClient/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Reliable.HttpClient)](https://www.nuget.org/packages/Reliable.HttpClient/)

## Caching Extension

[![NuGet Version](https://img.shields.io/nuget/v/Reliable.HttpClient.Caching)](https://www.nuget.org/packages/Reliable.HttpClient.Caching/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Reliable.HttpClient.Caching)](https://www.nuget.org/packages/Reliable.HttpClient.Caching/)

## Project Status

[![Build Status](https://github.com/akrisanov/Reliable.HttpClient/workflows/Build%20%26%20Test/badge.svg)](https://github.com/akrisanov/Reliable.HttpClient/actions)
[![License](https://img.shields.io/github/license/akrisanov/Reliable.HttpClient)](LICENSE)

A comprehensive resilience and caching ecosystem for HttpClient with built-in retry policies, circuit breakers, and intelligent response caching.
Based on [Polly](https://github.com/App-vNext/Polly) but with zero configuration required.

## Packages

| Package                           | Purpose                                  | Version                          |
|-----------------------------------|------------------------------------------|----------------------------------|
| **Reliable.HttpClient**           | Core resilience (retry + circuit breaker) | `dotnet add package Reliable.HttpClient` |
| **Reliable.HttpClient.Caching**   | HTTP response caching extension          | `dotnet add package Reliable.HttpClient.Caching` |

## Why Choose This Ecosystem?

- **Zero Configuration**: Works out of the box with sensible defaults
- **Complete Solution**: Resilience + Caching in one ecosystem
- **Lightweight**: Minimal overhead, maximum reliability
- **Production Ready**: Used by companies in production environments
- **Easy Integration**: One line of code to add resilience, two lines for caching
- **Secure**: SHA256-based cache keys prevent collisions and attacks
- **Flexible**: Use core resilience alone or add caching as needed

## Quick Start

### Basic Resilience

```bash
dotnet add package Reliable.HttpClient
```

```csharp
// Add resilience with zero configuration
builder.Services.AddHttpClient("myapi", c =>
{
    c.BaseAddress = new Uri("https://api.example.com");
})
.AddResilience(); // That's it! 🎉
```

### Resilience + Caching

```bash
dotnet add package Reliable.HttpClient.Caching
```

```csharp
// Add both resilience and caching
services.AddMemoryCache();
services.AddHttpClient<WeatherApiClient>()
    .AddResilience()  // Retry + Circuit breaker
    .AddMemoryCache<WeatherResponse>(options =>
    {
        options.DefaultExpiry = TimeSpan.FromMinutes(5);
    });
```

## What You Get

### Core Resilience (Reliable.HttpClient)

- **Retry Policy**: 3 attempts with exponential backoff + jitter
- **Circuit Breaker**: Opens after 5 failures, stays open for 1 minute
- **Smart Error Handling**: Retries on 5xx, 408, 429, and network errors
- **Zero Overhead**: Only activates when needed
- **Validation**: Configuration validation at startup

### Caching Extension (Reliable.HttpClient.Caching)

- **Automatic Caching**: Cache HTTP responses based on configurable rules
- **Smart Cache Keys**: SHA256-based generation with collision prevention
- **Multiple Providers**: Memory cache and custom cache providers
- **Cache Management**: Manual invalidation and clearing
- **Security**: Cryptographic hashing prevents cache poisoning
- **HTTP Standards**: Respects Cache-Control headers

### Custom Configuration (Optional)

```csharp
builder.Services.AddHttpClient<WeatherApiClient>()
    .AddResilience(options =>
    {
        options.Retry.MaxRetries = 5;
        options.CircuitBreaker.FailuresBeforeOpen = 10;
        // All other settings use sensible defaults
    });
```

## Trusted By

Organizations using Reliable.HttpClient in production:

[![PlanFact](https://raw.githubusercontent.com/akrisanov/Reliable.HttpClient/refs/heads/main/docs/assets/logos/planfact.png)](https://planfact.io)

## Documentation

- [Getting Started Guide](docs/getting-started.md) - Quick setup and basic usage
- [Configuration Reference](docs/configuration.md) - Complete options reference
- [Advanced Usage](docs/advanced-usage.md) - Advanced patterns and techniques
- [HTTP Caching Guide](docs/caching.md) - Complete caching documentation
- [Common Scenarios](docs/examples/common-scenarios.md) - Real-world examples

## Key Features

| Feature                  | Package | Description                     | Default                       |
|--------------------------|---------|---------------------------------|-------------------------------|
| Retry Policy             | Core    | Exponential backoff with jitter | 3 retries, 1s base delay      |
| Circuit Breaker          | Core    | Prevents cascading failures     | Opens after 5 failures        |
| Error Handling           | Core    | Smart retry decisions           | 5xx, 408, 429, network errors |
| Configuration Validation | Core    | Prevents invalid settings       | Automatic validation          |
| HTTP Response Caching    | Caching | Intelligent response caching    | 5-minute default expiry       |
| Cache Providers          | Caching | Memory & custom providers       | IMemoryCache integration      |
| Secure Cache Keys        | Caching | SHA256-based key generation     | Collision-resistant           |
| Multi-targeting          | Both    | .NET 6.0, 8.0, 9.0 support      | Latest frameworks             |

## Simple Example

### Core Resilience Only

```csharp
public class WeatherService
{
    private readonly HttpClient _httpClient;

    public WeatherService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("weather");
    }

    public async Task<WeatherData> GetWeatherAsync(string city)
    {
        // This call automatically retries on failures and respects circuit breaker
        var response = await _httpClient.GetAsync($"/weather?city={city}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WeatherData>();
    }
}

// Registration
builder.Services.AddHttpClient<WeatherService>("weather", c =>
{
    c.BaseAddress = new Uri("https://api.weather.com");
})
.AddResilience(); // Adds retry + circuit breaker
```

### With Caching

```csharp
public class WeatherService
{
    private readonly CachedHttpClient<WeatherData> _cachedClient;

    public WeatherService(CachedHttpClient<WeatherData> cachedClient)
    {
        _cachedClient = cachedClient;
    }

    public async Task<WeatherData> GetWeatherAsync(string city)
    {
        // This call includes retry, circuit breaker, AND caching
        return await _cachedClient.GetAsync($"/weather?city={city}");
    }
}

// Registration
services.AddMemoryCache();
services.AddHttpClient<WeatherService>()
    .AddResilience()
    .AddMemoryCache<WeatherData>(options =>
    {
        options.DefaultExpiry = TimeSpan.FromMinutes(10);
    });
```

## Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
