# Common Usage Scenarios

Real-world examples of using Reliable.HttpClient for resilience and caching in common scenarios.

## Quick Reference

### Core Resilience Examples

- [Scenario 1: E-commerce API Integration](#scenario-1-e-commerce-api-integration)
- [Scenario 2: Microservices Communication](#scenario-2-microservices-communication)
- [Scenario 3: External API with Rate Limiting](#scenario-3-external-api-with-rate-limiting)
- [Scenario 4: Legacy System Integration](#scenario-4-legacy-system-integration)

### Caching Examples

- [Scenario 5: Product Catalog with Caching](#scenario-5-product-catalog-with-caching)
- [Scenario 6: Configuration Service](#scenario-6-configuration-service)
- [Scenario 7: Weather API with Smart Caching](#scenario-7-weather-api-with-smart-caching)

---

## Scenario 1: E-commerce API Integration

Integrating with external payment and inventory APIs with different resilience requirements.

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Payment API - high reliability required, aggressive retries
        services.AddHttpClient<PaymentApiClient>(c =>
        {
            c.BaseAddress = new Uri("https://api.payments.com");
            c.DefaultRequestHeaders.Add("Authorization", "Bearer token");
            c.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddResilience(options =>
        {
            options.Retry.MaxRetries = 5;
            options.Retry.BaseDelay = TimeSpan.FromSeconds(2);
            options.Retry.MaxDelay = TimeSpan.FromMinutes(1);
            options.CircuitBreaker.FailuresBeforeOpen = 3;
            options.CircuitBreaker.OpenDuration = TimeSpan.FromMinutes(5);
        });

        // Inventory API - less critical, moderate resilience
        services.AddHttpClient<InventoryApiClient>(c =>
        {
            c.BaseAddress = new Uri("https://api.inventory.com");
            c.Timeout = TimeSpan.FromSeconds(15);
        })
        .AddResilience(options =>
        {
            options.Retry.MaxRetries = 3;
            options.CircuitBreaker.FailuresBeforeOpen = 5;
        });

        // Recommendation API - optional feature, minimal resilience
        services.AddHttpClient<RecommendationApiClient>(c =>
        {
            c.BaseAddress = new Uri("https://api.recommendations.com");
            c.Timeout = TimeSpan.FromSeconds(5);
        })
        .AddResilience(options =>
        {
            options.Retry.MaxRetries = 1;
            options.Retry.BaseDelay = TimeSpan.FromMilliseconds(500);
            options.CircuitBreaker.FailuresBeforeOpen = 10; // More tolerant
        });
    }
}

public class PaymentApiClient
{
    private readonly HttpClient _httpClient;

    public PaymentApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/payments", content);
        response.EnsureSuccessStatusCode();

        var resultJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PaymentResult>(resultJson);
    }
}
```

## Scenario 2: Microservices Communication

Service-to-service communication with different reliability requirements.

```csharp
public class ServiceConfiguration
{
    public static void ConfigureHttpClients(IServiceCollection services, IConfiguration config)
    {
        // User service - critical for authentication
        services.AddHttpClient("user-service", c =>
        {
            c.BaseAddress = new Uri(config["Services:UserService:BaseUrl"]);
            c.DefaultRequestHeaders.Add("Service-Name", "order-service");
        })
        .AddResilience(options =>
        {
            options.Retry.MaxRetries = 4;
            options.Retry.BackoffType = BackoffType.ExponentialWithJitter;
            options.CircuitBreaker.FailuresBeforeOpen = 5;
        });

        // Notification service - can fail gracefully
        services.AddHttpClient("notification-service", c =>
        {
            c.BaseAddress = new Uri(config["Services:NotificationService:BaseUrl"]);
        })
        .AddResilience(options =>
        {
            options.Retry.MaxRetries = 2;
            options.CircuitBreaker.FailuresBeforeOpen = 8;
        });

        // Analytics service - fire-and-forget
        services.AddHttpClient("analytics-service", c =>
        {
            c.BaseAddress = new Uri(config["Services:AnalyticsService:BaseUrl"]);
            c.Timeout = TimeSpan.FromSeconds(3);
        })
        .AddResilience(options =>
        {
            options.Retry.MaxRetries = 1;
            options.Retry.BaseDelay = TimeSpan.FromMilliseconds(200);
        });
    }
}

public class OrderService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IHttpClientFactory httpClientFactory, ILogger<OrderService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        // Critical: Validate user (must succeed)
        var userClient = _httpClientFactory.CreateClient("user-service");
        var userResponse = await userClient.GetAsync($"/users/{request.UserId}");
        userResponse.EnsureSuccessStatusCode();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Items = request.Items,
            CreatedAt = DateTime.UtcNow
        };

        // Important: Send notification (retries automatically)
        _ = Task.Run(async () =>
        {
            try
            {
                var notificationClient = _httpClientFactory.CreateClient("notification-service");
                await notificationClient.PostAsJsonAsync("/notifications", new
                {
                    UserId = order.UserId,
                    Message = $"Order {order.Id} created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send order notification for {OrderId}", order.Id);
            }
        });

        // Optional: Send analytics (fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                var analyticsClient = _httpClientFactory.CreateClient("analytics-service");
                await analyticsClient.PostAsJsonAsync("/events", new
                {
                    EventType = "OrderCreated",
                    OrderId = order.Id,
                    Timestamp = order.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to send analytics event for {OrderId}", order.Id);
            }
        });

        return order;
    }
}
```

## Scenario 3: External API with Rate Limiting

Handling APIs with rate limits and quotas.

```csharp
public class ExternalApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiClient> _logger;

    public ExternalApiClient(HttpClient httpClient, ILogger<ExternalApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);

            // Check rate limit headers
            if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var remaining))
            {
                if (int.TryParse(remaining.First(), out var remainingCount) && remainingCount < 10)
                {
                    _logger.LogWarning("API rate limit running low: {Remaining} requests remaining", remainingCount);
                }
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                // Get retry-after header if available
                if (response.Headers.RetryAfter?.Delta.HasValue == true)
                {
                    var retryAfter = response.Headers.RetryAfter.Delta.Value;
                    _logger.LogWarning("Rate limited. Retry after {RetryAfter}", retryAfter);
                }

                response.EnsureSuccessStatusCode(); // This will trigger retry
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<T>(json);

            return new ApiResponse<T> { Data = data, Success = true };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API request failed for endpoint {Endpoint}", endpoint);
            return new ApiResponse<T> { Success = false, Error = ex.Message };
        }
    }
}

// Configuration with special handling for rate limits
services.AddHttpClient<ExternalApiClient>(c =>
{
    c.BaseAddress = new Uri("https://api.external.com");
    c.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
    c.Timeout = TimeSpan.FromSeconds(30);
})
.AddResilience(options =>
{
    options.Retry.MaxRetries = 3;
    options.Retry.BaseDelay = TimeSpan.FromSeconds(5); // Longer delays for rate limits
    options.Retry.MaxDelay = TimeSpan.FromMinutes(2);
    options.CircuitBreaker.FailuresBeforeOpen = 8; // More tolerant of rate limits
    options.CircuitBreaker.OpenDuration = TimeSpan.FromMinutes(10);
});
```

## Scenario 4: Legacy System Integration

Working with unreliable legacy systems.

```csharp
public class LegacySystemClient
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LegacySystemClient> _logger;

    public LegacySystemClient(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<LegacySystemClient> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<CustomerData> GetCustomerAsync(int customerId)
    {
        var cacheKey = $"customer_{customerId}";

        // Check cache first
        if (_cache.TryGetValue(cacheKey, out CustomerData cachedCustomer))
        {
            _logger.LogDebug("Customer {CustomerId} found in cache", customerId);
            return cachedCustomer;
        }

        try
        {
            var response = await _httpClient.GetAsync($"/customers/{customerId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var customer = JsonSerializer.Deserialize<CustomerData>(json);

                // Cache successful response for 15 minutes
                _cache.Set(cacheKey, customer, TimeSpan.FromMinutes(15));

                return customer;
            }

            // If we have stale data, use it
            if (_cache.TryGetValue($"{cacheKey}_stale", out CustomerData staleCustomer))
            {
                _logger.LogWarning("Using stale data for customer {CustomerId}", customerId);
                return staleCustomer;
            }

            response.EnsureSuccessStatusCode();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get customer {CustomerId}", customerId);

            // Try to return stale data on error
            if (_cache.TryGetValue($"{cacheKey}_stale", out CustomerData emergencyCustomer))
            {
                _logger.LogInformation("Returning emergency cached data for customer {CustomerId}", customerId);
                return emergencyCustomer;
            }

            throw;
        }
    }
}

// Aggressive configuration for unreliable system
services.AddHttpClient<LegacySystemClient>(c =>
{
    c.BaseAddress = new Uri("http://legacy-system.internal");
    c.Timeout = TimeSpan.FromSeconds(45); // Legacy systems are slow
})
.AddResilience(options =>
{
    options.Retry.MaxRetries = 6; // More retries for flaky system
    options.Retry.BaseDelay = TimeSpan.FromSeconds(3);
    options.Retry.MaxDelay = TimeSpan.FromMinutes(5);
    options.CircuitBreaker.FailuresBeforeOpen = 15; // Very tolerant
    options.CircuitBreaker.OpenDuration = TimeSpan.FromMinutes(15);
});
```

## Scenario 5: Multi-Environment Configuration

Different resilience policies for different environments.

```csharp
public static class ResilienceConfiguration
{
    public static void ConfigureEnvironmentSpecificResilience(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        if (environment.IsDevelopment())
        {
            // Development: Fail fast, minimal retries
            services.AddHttpClient("api")
                .AddResilience(options =>
                {
                    options.Retry.MaxRetries = 1;
                    options.Retry.BaseDelay = TimeSpan.FromMilliseconds(100);
                    options.CircuitBreaker.FailuresBeforeOpen = 2;
                    options.CircuitBreaker.OpenDuration = TimeSpan.FromSeconds(30);
                });
        }
        else if (environment.IsStaging())
        {
            // Staging: Moderate resilience for testing
            services.AddHttpClient("api")
                .AddResilience(options =>
                {
                    options.Retry.MaxRetries = 2;
                    options.Retry.BaseDelay = TimeSpan.FromSeconds(1);
                    options.CircuitBreaker.FailuresBeforeOpen = 3;
                });
        }
        else // Production
        {
            // Production: Full resilience
            services.AddHttpClient("api")
                .AddResilience(); // Use defaults optimized for production
        }
    }
}

// In Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureEnvironmentSpecificResilience(
    builder.Environment,
    builder.Configuration);
```

## Scenario 6: Health Checks Integration

Combining with ASP.NET Core Health Checks.

```csharp
public class ApiHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ApiHealthCheck(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("api");
            var response = await client.GetAsync("/health", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("API is responsive");
            }

            return HealthCheckResult.Degraded($"API returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("API is not reachable", ex);
        }
    }
}

// Configuration
services.AddHttpClient("api", c => c.BaseAddress = new Uri("https://api.example.com"))
    .AddResilience(options =>
    {
        // Lighter resilience for health checks
        options.Retry.MaxRetries = 1;
        options.CircuitBreaker.FailuresBeforeOpen = 3;
    });

services.AddHealthChecks()
    .AddCheck<ApiHealthCheck>("api");
```

---

## Scenario 5: Product Catalog with Caching

E-commerce product catalog with intelligent caching for better performance.

```csharp
public class ProductCatalogService
{
    private readonly CachedHttpClient<Product> _productClient;
    private readonly CachedHttpClient<ProductList> _catalogClient;

    public ProductCatalogService(
        CachedHttpClient<Product> productClient,
        CachedHttpClient<ProductList> catalogClient)
    {
        _productClient = productClient;
        _catalogClient = catalogClient;
    }

    public async Task<Product> GetProductAsync(int productId)
    {
        return await _productClient.GetAsync($"/products/{productId}");
    }

    public async Task<ProductList> GetCategoryProductsAsync(string category)
    {
        return await _catalogClient.GetAsync($"/categories/{category}/products");
    }

    public async Task InvalidateProductAsync(int productId)
    {
        await _productClient.InvalidateAsync($"/products/{productId}");
    }
}

// Registration
services.AddMemoryCache();

// Individual products - longer cache (products don't change often)
services.AddHttpClient<ProductCatalogService>("products")
    .AddResilience()
    .AddMemoryCache<Product>(options =>
    {
        options.DefaultExpiry = TimeSpan.FromHours(1);
        options.VaryByHeaders = new[] { "Accept-Language", "Currency" };
        options.MaxCacheSize = 5000;
    });

// Product lists - shorter cache (inventory changes more frequently)
services.AddHttpClient<ProductCatalogService>("catalog")
    .AddResilience()
    .AddMemoryCache<ProductList>(options =>
    {
        options.DefaultExpiry = TimeSpan.FromMinutes(10);
        options.VaryByHeaders = new[] { "Accept-Language" };
    });
```

## Scenario 6: Configuration Service

Centralized configuration service with caching and fallback.

```csharp
public class ConfigurationService
{
    private readonly CachedHttpClient<AppConfig> _cachedClient;
    private readonly ILogger<ConfigurationService> _logger;
    private readonly IMemoryCache _fallbackCache;

    public async Task<AppConfig> GetConfigurationAsync(string environment)
    {
        try
        {
            var config = await _cachedClient.GetAsync($"/config/{environment}");

            // Store successful response as fallback
            _fallbackCache.Set($"fallback_config_{environment}", config, TimeSpan.FromDays(1));

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch configuration, using fallback");

            // Use fallback cache if available
            if (_fallbackCache.TryGetValue($"fallback_config_{environment}", out AppConfig fallback))
            {
                return fallback;
            }

            throw;
        }
    }
}

// Registration
services.AddMemoryCache();
services.AddHttpClient<ConfigurationService>()
    .AddResilience(options =>
    {
        options.Retry.MaxRetries = 3;
        options.CircuitBreaker.FailuresBeforeOpen = 5;
    })
    .AddMemoryCache<AppConfig>(options =>
    {
        options.DefaultExpiry = TimeSpan.FromMinutes(30);
        options.RespectCacheControlHeaders = true;
    });
```

## Scenario 7: Weather API with Smart Caching

Weather service that respects HTTP caching headers and handles rate limits.

```csharp
public class WeatherService
{
    private readonly CachedHttpClient<WeatherResponse> _cachedClient;

    public async Task<WeatherResponse> GetCurrentWeatherAsync(string city)
    {
        return await _cachedClient.GetAsync($"/current?city={city}");
    }

    public async Task<WeatherResponse> GetForecastAsync(string city, int days)
    {
        return await _cachedClient.GetAsync($"/forecast?city={city}&days={days}");
    }

    public async Task RefreshWeatherDataAsync(string city)
    {
        await _cachedClient.InvalidateAsync($"/current?city={city}");
        await _cachedClient.InvalidateAsync($"/forecast?city={city}");
    }
}

// Registration
services.AddMemoryCache();
services.AddHttpClient<WeatherService>(c =>
{
    c.BaseAddress = new Uri("https://api.weather.com/v1");
    c.DefaultRequestHeaders.Add("X-API-Key", "your-api-key");
})
.AddResilience(options =>
{
    // Handle rate limiting gracefully
    options.Retry.MaxRetries = 3;
    options.Retry.BaseDelay = TimeSpan.FromSeconds(2);
    options.CircuitBreaker.FailuresBeforeOpen = 10;
})
.AddMemoryCache<WeatherResponse>(options =>
{
    // Respect weather API cache headers
    options.RespectCacheControlHeaders = true;
    options.DefaultExpiry = TimeSpan.FromMinutes(15);

    // Cache per location
    options.VaryByHeaders = new[] { "Accept-Language" };
});
```
