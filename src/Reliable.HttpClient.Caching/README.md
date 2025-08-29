# Reliable.HttpClient.Caching

[![NuGet Version](https://img.shields.io/nuget/v/Reliable.HttpClient.Caching)](https://www.nuget.org/packages/Reliable.HttpClient.Caching/)

Intelligent HTTP response caching for [Reliable.HttpClient](https://www.nuget.org/packages/Reliable.HttpClient/)
with preset-based configuration and automatic memory management.

## Why Reliable.HttpClient.Caching?

- **Zero Configuration** – Works out of the box with sensible defaults
- **Preset-Based Setup** – 6 ready-made configurations for common scenarios
- **Automatic Dependencies** – No need to manually register `IMemoryCache`
- **Combined APIs** – Resilience + Caching in one method call
- **Performance Optimized** – Smart cache keys, configurable expiry, memory-efficient

## Installation

```bash
dotnet add package Reliable.HttpClient.Caching
```

> 📖 **[Full Documentation](../../docs/caching.md)** | **[Examples](../../docs/examples/)**

## Quick Start

### Option 1: Simple Setup (Recommended)

```csharp
// Zero configuration - just add resilience with caching
services.AddHttpClient<WeatherApiClient>()
    .AddResilienceWithMediumTermCache<WeatherResponse>(); // 10 minutes cache
```

### Option 2: Separate Registration

```csharp
// Step 1: Add resilience
services.AddHttpClient<WeatherApiClient>()
    .AddResilience();

// Step 2: Add caching (automatically registers IMemoryCache)
services.AddHttpClient<WeatherApiClient>()
    .AddMediumTermCache<WeatherResponse>(); // 10 minutes cache
```

### Option 3: Custom Configuration

```csharp
services.AddHttpClient<WeatherApiClient>()
    .AddResilienceWithCaching<WeatherResponse>(
        resilienceOptions => resilienceOptions.Retry.MaxRetries = 5,
        cacheOptions => cacheOptions.DefaultExpiry = TimeSpan.FromMinutes(15)
    );
```

## Preset-Based Configuration

Choose from ready-made cache presets for common scenarios:

```csharp
// Short-term caching (1 minute) - for frequently changing data
services.AddHttpClient<ApiClient>()
    .AddShortTermCache<ApiResponse>();

// Medium-term caching (10 minutes) - for moderately stable data
services.AddHttpClient<ApiClient>()
    .AddMediumTermCache<ApiResponse>();

// Long-term caching (1 hour) - for stable data
services.AddHttpClient<ApiClient>()
    .AddLongTermCache<ApiResponse>();

// High-performance caching (5 minutes, large cache) - for high-traffic APIs
services.AddHttpClient<ApiClient>()
    .AddHighPerformanceCache<ApiResponse>();

// Configuration caching (30 minutes) - for config data
services.AddHttpClient<ConfigClient>()
    .AddConfigurationCache<ConfigResponse>();
```

## Combined Resilience + Caching

```csharp
// Resilience with preset caching
services.AddHttpClient<ApiClient>()
    .AddResilienceWithShortTermCache<ApiResponse>(); // 1 minute

services.AddHttpClient<ApiClient>()
    .AddResilienceWithMediumTermCache<ApiResponse>(); // 10 minutes

services.AddHttpClient<ApiClient>()
    .AddResilienceWithLongTermCache<ApiResponse>(); // 1 hour

// Custom resilience with preset caching
services.AddHttpClient<ApiClient>()
    .AddResilienceWithCaching<ApiResponse>(
        HttpClientPresets.SlowExternalApi(), // Resilience preset
        CachePresets.MediumTerm               // Cache preset
    );
```

## Usage

```csharp
public class WeatherService
{
    private readonly CachedHttpClient<WeatherResponse> _client;

    public WeatherService(CachedHttpClient<WeatherResponse> client)
    {
        _client = client;
    }

    public async Task<WeatherResponse> GetWeatherAsync(string city)
    {
        return await _client.GetFromJsonAsync($"/weather?city={city}");
        // First call hits API, subsequent calls return cached response
    }
}
```

## When to Use Which Preset?

| Scenario | Preset | Cache Duration | Example |
|----------|--------|----------------|---------|
| **Real-time data** (prices, scores) | `ShortTerm` | 1 minute | Stock prices, live scores |
| **Regular updates** (news, feeds) | `MediumTerm` | 10 minutes | News articles, social feeds |
| **Reference data** (catalogs, lists) | `LongTerm` | 1 hour | Product catalogs, country lists |
| **High-traffic APIs** | `HighPerformance` | 5 min + large cache | Popular endpoints |
| **App configuration** | `Configuration` | 30 minutes | Feature flags, settings |
| **File downloads** | `FileDownload` | 2 hours | Documents, images |

## Advanced Usage

For advanced scenarios like custom cache providers, detailed configuration options, and troubleshooting,
see the **[comprehensive documentation](../../docs/caching.md)**.

## License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.
