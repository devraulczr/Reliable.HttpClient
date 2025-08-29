# Advanced Usage Patterns

Learn advanced techniques and patterns for using Reliable.HttpClient in complex scenarios.

## Multiple Named HttpClients

Configure different resilience policies for different services:

```csharp
// Fast internal service - minimal resilience
services.AddHttpClient("internal-api", c => c.BaseAddress = new Uri("http://internal-api"))
    .AddResilience(options =>
    {
        options.Retry.MaxRetries = 1;
        options.CircuitBreaker.FailuresBeforeOpen = 10;
    });

// External service - aggressive resilience
services.AddHttpClient("external-api", c => c.BaseAddress = new Uri("https://external-api.com"))
    .AddResilience(options =>
    {
        options.Retry.MaxRetries = 5;
        options.Retry.BaseDelay = TimeSpan.FromSeconds(1);
        options.CircuitBreaker.FailuresBeforeOpen = 3;
        options.CircuitBreaker.OpenDuration = TimeSpan.FromMinutes(5);
    });
```

## Typed HttpClients

Use with typed HttpClient pattern for better encapsulation:

```csharp
public class WeatherApiClient
{
    private readonly HttpClient _httpClient;

    public WeatherApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherData> GetWeatherAsync(string city)
    {
        var response = await _httpClient.GetAsync($"/weather?city={city}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WeatherData>();
    }
}

// Registration
services.AddHttpClient<WeatherApiClient>(c =>
{
    c.BaseAddress = new Uri("https://api.weather.com");
    c.DefaultRequestHeaders.Add("Api-Key", "your-key");
})
.AddResilience();
```

## Custom Error Handling

Override which errors should trigger retries:

```csharp
public class CustomHttpResponseHandler : HttpResponseHandlerBase
{
    protected override bool ShouldRetry(HttpResponseMessage response)
    {
        // Retry on server errors and rate limiting
        if (response.StatusCode >= HttpStatusCode.InternalServerError)
            return true;

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            return true;

        // Don't retry on 4xx client errors (except 408, 429)
        if (response.StatusCode >= HttpStatusCode.BadRequest &&
            response.StatusCode < HttpStatusCode.InternalServerError)
            return false;

        return false;
    }
}

// Registration
services.AddSingleton<IHttpResponseHandler, CustomHttpResponseHandler>();
services.AddHttpClient("api").AddResilience();
```

## Conditional Resilience

Apply resilience based on environment or configuration:

```csharp
public static class ServiceCollectionExtensions
{
    public static IHttpClientBuilder AddConditionalResilience(
        this IHttpClientBuilder builder,
        IConfiguration configuration)
    {
        var resilienceEnabled = configuration.GetValue<bool>("Features:ResilienceEnabled", true);

        if (resilienceEnabled)
        {
            return builder.AddResilience();
        }

        return builder;
    }
}

// Usage
services.AddHttpClient("api")
    .AddConditionalResilience(configuration);
```

## Monitoring and Observability

### Logging Circuit Breaker Events

```csharp
public class ObservableCircuitBreakerOptions : CircuitBreakerOptions
{
    public Action<string> OnCircuitOpened { get; set; }
    public Action<string> OnCircuitClosed { get; set; }
    public Action<string> OnCircuitHalfOpened { get; set; }
}

// In your configuration
.AddResilience(options =>
{
    options.CircuitBreaker.OnCircuitOpened = (name) =>
        logger.LogWarning("Circuit breaker {Name} opened", name);
    options.CircuitBreaker.OnCircuitClosed = (name) =>
        logger.LogInformation("Circuit breaker {Name} closed", name);
});
```

### Custom Metrics

```csharp
public class MetricsHttpResponseHandler : HttpResponseHandlerBase
{
    private readonly IMetrics _metrics;

    public MetricsHttpResponseHandler(IMetrics metrics)
    {
        _metrics = metrics;
    }

    protected override bool ShouldRetry(HttpResponseMessage response)
    {
        _metrics.Counter("http_requests_total")
            .WithTag("status_code", ((int)response.StatusCode).ToString())
            .Increment();

        return base.ShouldRetry(response);
    }
}
```

## Performance Optimization

### Reusing HttpClient Instances

```csharp
// ✅ Good - reuse HttpClient instances
services.AddHttpClient("shared-api")
    .AddResilience()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        MaxConnectionsPerServer = 100,
        PooledConnectionLifetime = TimeSpan.FromMinutes(10)
    });

// ❌ Bad - creating new HttpClient instances
public class BadService
{
    public async Task<string> GetDataAsync()
    {
        using var client = new HttpClient(); // Don't do this!
        return await client.GetStringAsync("https://api.example.com/data");
    }
}
```

### Connection Pooling

```csharp
services.AddHttpClient("optimized-api")
    .AddResilience()
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        MaxConnectionsPerServer = 50,
        PooledConnectionLifetime = TimeSpan.FromMinutes(15),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
        EnableMultipleHttp2Connections = true
    });
```

## Testing with Resilience

### Unit Testing

```csharp
[Test]
public async Task Should_Retry_On_Server_Error()
{
    var mockHandler = new Mock<HttpMessageHandler>();

    // First call fails, second succeeds
    mockHandler.SetupSequence(h => h.SendAsync(
            It.IsAny<HttpRequestMessage>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError))
        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("success")
        });

    var httpClient = new HttpClient(mockHandler.Object);
    var service = new MyService(httpClient);

    var result = await service.GetDataAsync();

    result.Should().Be("success");
    mockHandler.Verify(h => h.SendAsync(
        It.IsAny<HttpRequestMessage>(),
        It.IsAny<CancellationToken>()), Times.Exactly(2));
}
```

### Integration Testing

```csharp
[Test]
public async Task Should_Handle_Real_Network_Failures()
{
    var services = new ServiceCollection();
    services.AddHttpClient<TestApiClient>(c =>
    {
        c.BaseAddress = new Uri("https://httpstat.us");
        c.Timeout = TimeSpan.FromSeconds(2);
    })
    .AddResilience(options =>
    {
        options.Retry.MaxRetries = 2;
        options.Retry.BaseDelay = TimeSpan.FromMilliseconds(100);
    });

    var provider = services.BuildServiceProvider();
    var client = provider.GetRequiredService<TestApiClient>();

    // This will retry on 500 errors
    var response = await client.GetAsync("/500");

    response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
}
```

## Common Patterns

### Graceful Degradation

```csharp
public class WeatherService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    public async Task<WeatherData> GetWeatherAsync(string city)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/weather?city={city}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<WeatherData>();
                _cache.Set($"weather_{city}", data, TimeSpan.FromMinutes(30));
                return data;
            }
        }
        catch (HttpRequestException)
        {
            // Fallback to cache if available
            if (_cache.TryGetValue($"weather_{city}", out WeatherData cachedData))
            {
                return cachedData;
            }
        }

        // Return default/fallback data
        return new WeatherData { City = city, Temperature = "Unknown" };
    }
}
```

### Bulkhead Pattern

Isolate different types of operations:

```csharp
// Separate clients for different operation types
services.AddHttpClient("read-operations")
    .AddResilience(options =>
    {
        options.Retry.MaxRetries = 5; // More aggressive for reads
    });

services.AddHttpClient("write-operations")
    .AddResilience(options =>
    {
        options.Retry.MaxRetries = 1; // Conservative for writes
        options.CircuitBreaker.FailuresBeforeOpen = 3;
    });
```
