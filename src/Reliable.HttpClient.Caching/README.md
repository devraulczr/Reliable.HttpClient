# Reliable.HttpClient.Caching

[![NuGet Version](https://img.shields.io/nuget/v/Reliable.HttpClient.Caching)](https://www.nuget.org/packages/Reliable.HttpClient.Caching/)

Caching extensions for [Reliable.HttpClient](https://www.nuget.org/packages/Reliable.HttpClient/)
with support for memory cache and custom cache providers.

## Features

- **Automatic HTTP Response Caching** – Cache responses based on configurable rules
- **Multiple Cache Providers** – Memory cache and custom cache providers support
- **Flexible Configuration** – TTL, cache keys, status codes, HTTP methods
- **Smart Cache Keys** – Automatic generation based on request method, URI, and headers
- **Cache Management** – Manual cache invalidation and clearing
- **Logging Support** – Built-in logging for cache hits/misses

## Installation

```bash
dotnet add package Reliable.HttpClient.Caching
```

## Quick Start

### Basic Memory Caching

```csharp
// Registration - Method 1: Separate registration (recommended)
services.AddMemoryCache();  // Required for memory caching
services.AddHttpClient<WeatherApiClient>()
    .AddResilience();  // From Reliable.HttpClient

services.AddHttpClientCaching<WeatherApiClient, WeatherResponse>(options =>
{
    options.DefaultExpiry = TimeSpan.FromMinutes(5);
    options.MaxCacheSize = 1_000;
});

// Registration - Method 2: Fluent API (alternative)
services.AddHttpClient<WeatherApiClient>()
    .AddResilience()  // From Reliable.HttpClient
    .AddMemoryCache<WeatherResponse>(options =>
    {
        options.DefaultExpiry = TimeSpan.FromMinutes(5);
        options.MaxCacheSize = 1_000;
    });

// Usage
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

### Advanced Configuration

```csharp
services.AddMemoryCache();  // Required
services.AddHttpClient<ApiClient>()
    .AddResilience();  // Optional: add resilience policies

services.AddHttpClientCaching<ApiClient, ApiResponse>(options =>
{
    options.DefaultExpiry = TimeSpan.FromMinutes(10);

    // Custom cache key generation
    options.KeyGenerator = new CustomCacheKeyGenerator();

    // Only cache successful responses
    options.ShouldCache = (request, response) =>
        response.IsSuccessStatusCode &&
        !request.Headers.Authorization?.Parameter?.Contains("temp");

    // Custom expiry based on response
    options.GetExpiry = (request, response) =>
    {
        if (response.Headers.CacheControl?.MaxAge.HasValue == true)
            return response.Headers.CacheControl.MaxAge.Value;
        return TimeSpan.FromMinutes(5);
    };

    // Cache only specific methods and status codes
    options.CacheableMethods = new HashSet<HttpMethod> { HttpMethod.Get };
    options.CacheableStatusCodes = new HashSet<HttpStatusCode>
    {
        HttpStatusCode.OK,
        HttpStatusCode.NotModified
    };
});
```

### Manual Cache Operations

```csharp
public class ApiService
{
    private readonly CachedHttpClient<ApiResponse> _client;

    public async Task<ApiResponse> GetDataAsync(string id)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/data/{id}");

        return await _client.SendAsync(request, async response =>
        {
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiResponse>(json);
        });
    }

    public async Task InvalidateDataAsync(string id)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/data/{id}");
        await _client.RemoveFromCacheAsync(request);
    }

    public async Task ClearAllCacheAsync()
    {
        await _client.ClearCacheAsync();
    }
}
```

### Custom Cache Provider

```csharp
public class RedisCacheProvider<TResponse> : IHttpResponseCache<TResponse>
{
    private readonly IDatabase _database;

    public async Task<TResponse?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<TResponse>(value) : default;
    }

    public async Task SetAsync(string key, TResponse value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, json, expiry);
    }

    // ... implement other methods
}

// Registration
services.AddMemoryCache();  // Still required for fallback
services.AddHttpClient<ApiClient>();

// Option 1: Using AddHttpClientCaching with custom provider
services.AddScoped<IHttpResponseCache<ApiResponse>, RedisCacheProvider<ApiResponse>>();
services.AddHttpClientCaching<ApiClient, ApiResponse>();

// Option 2: Using fluent API (if available)
services.AddHttpClient<ApiClient>()
    .AddCache<ApiResponse, RedisCacheProvider<ApiResponse>>();
```

## Configuration Options

| Property                | Type                                                      | Default                        | Description                              |
|-------------------------|-----------------------------------------------------------|--------------------------------|------------------------------------------|
| `DefaultExpiry`         | `TimeSpan`                                                | 5 minutes                      | Default cache expiration time            |
| `MaxCacheSize`          | `int?`                                                    | 1_000                          | Maximum cached items (memory cache only) |
| `KeyGenerator`          | `ICacheKeyGenerator`                                      | `DefaultCacheKeyGenerator`     | Cache key generation strategy            |
| `CacheableStatusCodes`  | `HashSet<HttpStatusCode>`                                 | 200, 304, 206                  | HTTP status codes to cache               |
| `CacheableMethods`      | `HashSet<HttpMethod>`                                     | GET, HEAD                      | HTTP methods to cache                    |
| `ShouldCache`           | `Func<HttpRequestMessage, HttpResponseMessage, bool>`     | Cache-Control aware            | Custom caching logic                     |
| `GetExpiry`             | `Func<HttpRequestMessage, HttpResponseMessage, TimeSpan>` | Cache-Control aware            | Dynamic expiry calculation               |

## Cache Key Generation

The default cache key generator creates keys using:

- HTTP method (GET, POST, etc.)
- Request URI
- Authorization header (if present)

Example: `GET:https://api.example.com/weather?city=London|auth:Bearer`

### Custom Cache Key Generator

```csharp
public class CustomCacheKeyGenerator : ICacheKeyGenerator
{
    public string GenerateKey(HttpRequestMessage request)
    {
        var method = request.Method.Method;
        var uri = request.RequestUri?.ToString() ?? string.Empty;

        // Include user-specific headers
        var userId = request.Headers.GetValues("X-User-Id").FirstOrDefault();

        return $"{method}:{uri}:user:{userId}";
    }
}
```

## Best Practices

### 1. Choose Appropriate Cache Duration

```csharp
// Static data - longer cache
options.DefaultExpiry = TimeSpan.FromHours(1);

// Dynamic data - shorter cache
options.DefaultExpiry = TimeSpan.FromMinutes(5);

// Real-time data - very short cache
options.DefaultExpiry = TimeSpan.FromSeconds(30);
```

### 2. Consider Cache Size

```csharp
// Limit memory usage
options.MaxCacheSize = 1_000; // Adjust based on available memory
```

### 3. Cache Invalidation Strategy

```csharp
// Invalidate on data updates
public async Task UpdateUserAsync(User user)
{
    await _userRepository.UpdateAsync(user);

    // Remove from cache
    var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{user.Id}");
    await _cachedClient.RemoveFromCacheAsync(request);
}
```

### 4. Handle Cache Failures Gracefully

```csharp
options.ShouldCache = (request, response) =>
{
    try
    {
        return response.IsSuccessStatusCode &&
               response.Content.Headers.ContentLength < 1_000_000; // Don't cache large responses
    }
    catch
    {
        return false; // Skip caching on errors
    }
};
```

## Integration with Reliable.HttpClient

Caching works seamlessly with resilience policies:

```csharp
services.AddMemoryCache();  // Required
services.AddHttpClient<ApiClient>()
    .AddResilience(options =>  // Retry + Circuit Breaker
    {
        options.Retry.MaxRetries = 3;
        options.CircuitBreaker.FailuresBeforeOpen = 5;
    });

services.AddHttpClientCaching<ApiClient, ApiResponse>(options =>  // + Caching
{
    options.DefaultExpiry = TimeSpan.FromMinutes(5);
});
```

**Execution Order**: Cache Check → Resilience Policies → HTTP Request → Cache Store

## Performance Considerations

- **Memory Usage**: Monitor cache size, especially for large responses
- **Serialization**: JSON serialization overhead for complex objects
- **Thread Safety**: All cache providers are thread-safe
- **GC Pressure**: Use appropriate expiry times to avoid memory leaks

## License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.
