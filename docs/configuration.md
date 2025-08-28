# Configuration Reference

Complete reference for all configuration options in HttpClient.Resilience.

## Overview

HttpClient.Resilience provides sensible defaults that work well for most scenarios.
You only need to configure options that differ from these defaults.

## Main Configuration Class

### HttpClientOptions

| Property         | Type                     | Default   | Description                       |
|------------------|--------------------------|-----------|-----------------------------------|
| `Retry`          | `RetryOptions`           | See below | Retry policy configuration        |
| `CircuitBreaker` | `CircuitBreakerOptions`  | See below | Circuit breaker configuration     |

```csharp
.AddResilience(options =>
{
    // Configure retry settings
    options.Retry.MaxRetries = 5;

    // Configure circuit breaker settings
    options.CircuitBreaker.FailuresBeforeOpen = 10;
});
```

## Retry Configuration

### RetryOptions

| Property         | Type                     | Default     | Description                       |
|------------------|--------------------------|-------------|-----------------------------------|
| `MaxRetries`     | `int`                    | `3`         | Maximum number of retry attempts  |
| `BaseDelay`      | `TimeSpan`               | `1 second`. | Base delay between retries        |
| `MaxDelay`       | `TimeSpan`               | `30 seconds`| Maximum delay between retries     |
| `JitterFactor`   | `double`                 | `0.25`      | Randomization factor (0.0 to 1.0) |

#### Retry Behavior

The retry policy uses exponential backoff with jitter by default. The actual delay is calculated
using the base delay, exponential backoff, and jitter factor to prevent thundering herd effects.

#### Retry Configuration Example

```csharp
options.Retry = new RetryOptions
{
    MaxRetries = 5,
    BaseDelay = TimeSpan.FromSeconds(1),
    MaxDelay = TimeSpan.FromMinutes(2),
    JitterFactor = 0.3
};
```

## Circuit Breaker Configuration

### CircuitBreakerOptions

| Property             | Type        | Default    | Description                                   |
|----------------------|-------------|------------|-----------------------------------------------|
| `Enabled`            | `bool`      | `true`     | Whether circuit breaker is enabled            |
| `FailuresBeforeOpen` | `int`       | `5`        | Number of consecutive failures before opening |
| `OpenDuration`       | `TimeSpan`  | `1 minute` | How long circuit stays open                   |

#### Circuit Breaker Configuration Example

```csharp
options.CircuitBreaker = new CircuitBreakerOptions
{
    FailuresBeforeOpen = 10,
    OpenDuration = TimeSpan.FromMinutes(5),
    Enabled = true
};
```

## Validation

All configuration is automatically validated when the HttpClient is created.
Invalid configurations throw `ArgumentException` with descriptive error messages.

### Common Validation Rules

- `MaxRetries` must be ≥ 0
- `BaseDelay` must be > 0
- `MaxDelay` must be > 0
- `BaseDelay` cannot be > `MaxDelay`
- `JitterFactor` must be between 0.0 and 1.0
- `FailuresBeforeOpen` must be > 0
- `OpenDuration` must be > 0

## Preset Configurations

### Conservative (Default)

Good for most production scenarios:

```csharp
.AddResilience(); // Uses defaults
```

### Aggressive

For high-traffic, fault-tolerant scenarios:

```csharp
.AddResilience(options =>
{
    options.Retry.MaxRetries = 5;
    options.Retry.BaseDelay = TimeSpan.FromMilliseconds(500);
    options.CircuitBreaker.FailuresBeforeOpen = 10;
    options.CircuitBreaker.OpenDuration = TimeSpan.FromMinutes(2);
});
```

### Minimal

For scenarios where you want basic resilience with minimal delays:

```csharp
.AddResilience(options =>
{
    options.Retry.MaxRetries = 2;
    options.Retry.BaseDelay = TimeSpan.FromMilliseconds(100);
    options.Retry.MaxDelay = TimeSpan.FromSeconds(5);
    options.CircuitBreaker.FailuresBeforeOpen = 3;
});
```

## Environment-Specific Configuration

### Development

```csharp
#if DEBUG
.AddResilience(options =>
{
    options.Retry.MaxRetries = 1; // Fail fast in development
    options.CircuitBreaker.FailuresBeforeOpen = 2;
});
#else
.AddResilience(); // Use defaults in production
#endif
```

### Configuration from appsettings.json

```json
{
  "HttpClient": {
    "Retry": {
      "MaxRetries": 3,
      "BaseDelay": "00:00:02",
      "MaxDelay": "00:00:30"
    },
    "CircuitBreaker": {
      "FailuresBeforeOpen": 5,
      "DurationOfBreak": "00:01:00"
    }
  }
}
```

```csharp
var config = configuration.GetSection("HttpClient").Get<HttpClientOptions>();
services.AddHttpClient("api").AddResilience(config);
```
