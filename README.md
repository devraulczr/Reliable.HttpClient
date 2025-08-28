# HttpClient.Resilience

[![NuGet Version](https://img.shields.io/nuget/v/HttpClient.Resilience)](https://www.nuget.org/packages/HttpClient.Resilience/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/HttpClient.Resilience)](https://www.nuget.org/packages/HttpClient.Resilience/)
[![Build Status](https://github.com/akrisanov/HttpClient.Resilience/workflows/Build%20%26%20Test/badge.svg)](https://github.com/akrisanov/HttpClient.Resilience/actions)
[![License](https://img.shields.io/github/license/akrisanov/HttpClient.Resilience)](LICENSE)

A lightweight, easy-to-use resilience library for HttpClient with built-in retry policies and circuit breakers.
Based on [Polly](https://github.com/App-vNext/Polly) but with zero configuration required.

## Why HttpClient.Resilience?

- **Zero Configuration**: Works out of the box with sensible defaults
- **Lightweight**: Minimal overhead, maximum reliability
- **Production Ready**: Used by companies in production environments
- **Easy Integration**: One line of code to add resilience
- **Flexible**: Customize only what you need

## Installation & Quick Start

```bash
dotnet add package HttpClient.Resilience
```

```csharp
// Add resilience with zero configuration
builder.Services.AddHttpClient("myapi", c =>
{
    c.BaseAddress = new Uri("https://api.example.com");
})
.AddResilience(); // That's it! 🎉
```

### What You Get

Your HttpClient automatically gains:

- **Retry Policy**: 3 attempts with exponential backoff + jitter
- **Circuit Breaker**: Opens after 5 failures, stays open for 1 minute
- **Smart Error Handling**: Retries on 5xx, 408, 429, and network errors
- **Zero Overhead**: Only activates when needed

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

Organizations using HttpClient.Resilience in production:

[![PlanFact](docs/assets/logos/planfact.png)](https://planfact.io)

## Documentation

- 📚 [Getting Started Guide](docs/getting-started.md) - Quick setup and basic usage
- ⚙️ [Configuration Reference](docs/configuration.md) - Complete options reference
- 🚀 [Advanced Usage](docs/advanced-usage.md) - Advanced patterns and techniques
- 💡 [Common Scenarios](docs/examples/common-scenarios.md) - Real-world examples

## Key Features

| Feature                  | Description                     | Default                       |
|--------------------------|---------------------------------|-------------------------------|
| Retry Policy             | Exponential backoff with jitter | 3 retries, 1s base delay      |
| Circuit Breaker          | Prevents cascading failures     | Opens after 5 failures        |
| Error Handling           | Smart retry decisions           | 5xx, 408, 429, network errors |
| Configuration Validation | Prevents invalid settings       | Automatic validation          |
| Multi-targeting          | .NET 6.0, 8.0, 9.0 support      | Latest frameworks             |

## Simple Example

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

## Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
