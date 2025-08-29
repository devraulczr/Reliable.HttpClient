# Reliable.HttpClient

## Core Package

[![NuGet Version](https://img.shields.io/nuget/v/Reliable.HttpClient)](https://www.nuget.org/packages/Reliable.HttpClient/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Reliable.HttpClient)](https://www.nuget.org/packages/Reliable.HttpClient/)

## Caching Extension

[![NuGet Version](https://img.shields.io/nuget/v/Reliable.HttpClient.Caching)](https://www.nuget.org/packages/Reliable.HttpClient.Caching/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Reliable.HttpClient.Caching)](https://www.nuget.org/packages/Reliable.HttpClient.Caching/)

## Project Status

[![Build Status](https://github.com/akrisanov/Reliable.HttpClient/workflows/Build%20%26%20Test/badge.svg)](https://github.com/akrisanov/Reliable.HttpClient/actions)
[![codecov](https://codecov.io/gh/akrisanov/Reliable.HttpClient/branch/main/graph/badge.svg)](https://codecov.io/gh/akrisanov/Reliable.HttpClient)
[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%208.0%20%7C%209.0-blue)](https://dotnet.microsoft.com/)
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

### 1️⃣ Install & Add Resilience (2 lines of code)

```bash
dotnet add package Reliable.HttpClient
```

```csharp
builder.Services.AddHttpClient<WeatherApiClient>(c =>
{
    c.BaseAddress = new Uri("https://api.weather.com");
})
.AddResilience(); // ✨ That's it! Zero configuration needed
```

**You now have:**

- Automatic retries (3 attempts with smart backoff)
- Circuit breaker (prevents cascading failures)
- Smart error handling (5xx, timeouts, rate limits)

### 2️⃣ Add Caching (Optional)

Want to cache responses? Add one more package and line:

```bash
dotnet add package Reliable.HttpClient.Caching
```

```csharp
builder.Services.AddMemoryCache(); // Standard .NET caching

builder.Services.AddHttpClient<WeatherApiClient>(c =>
{
    c.BaseAddress = new Uri("https://api.weather.com");
})
.AddResilience()
.AddMemoryCache<WeatherResponse>(); // ✨ Intelligent caching added!
```

**Now you also have:**

- Automatic response caching (5-minute default)
- Smart cache keys (collision-resistant SHA256)
- Manual cache invalidation

### 3️⃣ Use Your Client

```csharp
public class WeatherService
{
    private readonly HttpClient _client;

    public WeatherService(IHttpClientFactory factory)
    {
        _client = factory.CreateClient<WeatherApiClient>();
    }

    public async Task<WeatherResponse> GetWeatherAsync(string city)
    {
        // This call now has retry, circuit breaker, AND caching!
        var response = await _client.GetAsync($"/weather?city={city}");
        return await response.Content.ReadFromJsonAsync<WeatherResponse>();
    }
}
```

> 🎯 **That's it!** You're production-ready with 2-3 lines of configuration.

## What You Get

- ✅ **Retry Policy**: 3 attempts with exponential backoff + jitter
- ✅ **Circuit Breaker**: Opens after 5 failures, stays open for 1 minute
- ✅ **Smart Error Handling**: Retries on 5xx, 408, 429, and network errors
- ✅ **HTTP Response Caching**: 5-minute default expiry with SHA256 cache keys
- ✅ **Multiple Configuration Options**: Zero-config, presets, or custom setup
- ✅ **Production Ready**: Used by companies in production environments

> 📖 **See [Key Features Table](docs/README.md#key-features) for complete feature comparison**

## Advanced Configuration (Optional)

Need custom settings? Multiple ways to configure:

```csharp
// Option 1: Traditional configuration
builder.Services.AddHttpClient<ApiClient>()
    .AddResilience(options => options.Retry.MaxRetries = 5);

// Option 2: Fluent builder
builder.Services.AddHttpClient<ApiClient>()
    .AddResilience(builder => builder.WithRetry(r => r.WithMaxRetries(5)));

// Option 3: Ready-made presets
builder.Services.AddHttpClient<ApiClient>()
    .AddResilience(HttpClientPresets.SlowExternalApi());
```

> 📖 **See [Configuration Guide](docs/configuration.md) for complete configuration options**## Trusted By

Organizations using Reliable.HttpClient in production:

[![PlanFact](https://raw.githubusercontent.com/akrisanov/Reliable.HttpClient/refs/heads/main/docs/assets/logos/planfact.png)](https://planfact.io)

## Documentation

- [Getting Started Guide](docs/getting-started.md) - Quick setup and basic usage
- [Configuration Reference](docs/configuration.md) - Complete options reference
- [Advanced Usage](docs/advanced-usage.md) - Advanced patterns and techniques
- [HTTP Caching Guide](docs/caching.md) - Complete caching documentation
- [Common Scenarios](docs/examples/common-scenarios.md) - Real-world examples
- [Complete Feature List](docs/README.md#key-features) - Detailed feature comparison

## Complete Example

Here's a complete working example showing both packages in action:

### The Service

```csharp
public class WeatherService
{
    private readonly HttpClient _httpClient;

    public WeatherService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient<WeatherApiClient>();
    }

    public async Task<WeatherData> GetWeatherAsync(string city)
    {
        // This call has retry, circuit breaker, AND caching automatically!
        var response = await _httpClient.GetAsync($"/weather?city={city}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WeatherData>();
    }
}
```

### The Registration

```csharp
// In Program.cs
services.AddMemoryCache();

services.AddHttpClient<WeatherApiClient>(c =>
{
    c.BaseAddress = new Uri("https://api.weather.com");
    c.DefaultRequestHeaders.Add("API-Key", "your-key");
})
.AddResilience()                    // Retry + Circuit breaker
.AddMemoryCache<WeatherData>();     // Response caching
```

**That's it!** Production-ready HTTP client with resilience and caching in just a few lines. 🚀

## Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
