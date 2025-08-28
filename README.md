# HttpClient.Resilience

[![NuGet](https://img.shields.io/nuget/v/HttpClient.Resilience.svg)](https://www.nuget.org/packages/HttpClient.Resilience)
[![NuGet Downloads](https://img.shields.io/nuget/dt/HttpClient.Resilience.svg)](https://www.nuget.org/packages/HttpClient.Resilience)
[![Build & Test](https://github.com/akrisanov/HttpClient.Resilience/actions/workflows/ci.yml/badge.svg)](https://github.com/akrisanov/HttpClient.Resilience/actions/workflows/ci.yml)
[![Release](https://github.com/akrisanov/HttpClient.Resilience/actions/workflows/release.yml/badge.svg)](https://github.com/akrisanov/HttpClient.Resilience/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## Overview

HttpClient.Resilience is a **configuration-first wrapper** around Polly that provides opinionated defaults
and simplified setup for common HttpClient resilience scenarios. It focuses on developer experience
for typical retry and circuit breaker patterns while maintaining the power of Polly underneath.

## Supported Frameworks

- .NET 6 (LTS)
- .NET 8 (LTS)
- .NET 9 (Current)

## When to use this library

- ✅ You want opinionated defaults for common scenarios
- ✅ You prefer configuration over code (appsettings.json-driven)
- ✅ You need simple retry + circuit breaker patterns
- ✅ Your team prefers conventions over detailed policy customization
- ❌ You need advanced Polly features (cache, bulkhead, policy wrapping)
- ❌ You want minimal dependencies
- ❌ You require fine-grained control over policy behavior

## Features

- Retry policies with configurable jitter and support for `Retry-After` headers
- Circuit breaker pattern to prevent cascading failures
- Configurable `HttpClient` options
- Seamless integration with dependency injection (DI)
- Base response handler abstraction for extensibility

## Installation

Install the package via NuGet:

```bash
dotnet add package HttpClient.Resilience
```

Or via Package Manager Console:

```powershell
Install-Package HttpClient.Resilience
```

## Quick Start

Register an `HttpClient` with resilience policies in your `IServiceCollection`:

```csharp
builder.Services.AddHttpClient("myapi", c =>
{
    c.BaseAddress = new Uri("https://api.example.com");
})
.services
    .AddHttpClient<WeatherApiClient>()
    .AddResilience(options =>
    {
        // Configure retry behavior
        options.Retry.MaxRetries = 3;
        options.Retry.BaseDelay = TimeSpan.FromMilliseconds(500);
        options.Retry.MaxDelay = TimeSpan.FromSeconds(30);
        options.Retry.JitterFactor = 0.2;

        // Configure circuit breaker
        options.CircuitBreaker.FailuresBeforeOpen = 5;
        options.CircuitBreaker.OpenDuration = TimeSpan.FromMinutes(1);
    });
```

Use the client as usual:

```csharp
var client = httpClientFactory.CreateClient("myapi");
var response = await client.GetAsync("/endpoint");
response.EnsureSuccessStatusCode();
```

## Advanced Usage

Customize retry behavior with exponential backoff and jitter:

```csharp
options.Retry.MaxRetries = 5;
options.Retry.BaseDelay = TimeSpan.FromSeconds(1);
options.Retry.JitterFactor = 0.3;
```

Configure circuit breaker thresholds and reset intervals:

```csharp
options.CircuitBreaker.FailuresBeforeOpen = 10;
options.CircuitBreaker.OpenDuration = TimeSpan.FromMinutes(1);
```

Use response handlers for automatic JSON deserialization:

```csharp
// Register the response handler
services.AddScoped<JsonResponseHandler<WeatherData>>();

// Use it to process responses
var handler = serviceProvider.GetRequiredService<JsonResponseHandler<WeatherData>>();
var weatherData = await handler.HandleAsync(response);
```

## Telemetry & Logging

HttpClient.Resilience currently uses `ILogger` for detailed logging of retries, circuit breaker
state changes, and related events. Enable logging in your application to monitor resilience events
and diagnose issues effectively.

Currently only ILogger-based logging is implemented. Custom metrics/traces are planned.

Integration with OpenTelemetry for metrics and tracing is planned. The following code snippet is
illustrative and future-ready for OpenTelemetry integration:

```csharp
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder.AddHttpClientInstrumentation();
        tracerProviderBuilder.AddSource("HttpClient.Resilience");
    })
    .WithMetrics(meterProviderBuilder =>
    {
        meterProviderBuilder.AddHttpClientInstrumentation();
        meterProviderBuilder.AddMeter("HttpClient.Resilience");
    });
```

## Roadmap / Status

- Current version: pre-release (1.0.0-alpha1)

### Planned features

- Per-try timeout policy
- Response handlers collection API (`options.ResponseHandlers.Add(...)`)
- Extended response handler implementations (XML, custom formats)
- Structured logging with correlation IDs
- Health checks for circuit breaker and overall system state
- OpenTelemetry metrics and tracing integration
- Configuration validation
- Benchmarks and performance analysis
- Bulkhead isolation policy

Contributions and feedback are welcome to help shape the API and features.

## Contributing

Contributions are encouraged! Please open issues or pull requests on the GitHub repository.
Follow the established code style and include tests for new features or bug fixes.

_Keywords: dotnet, csharp, httpclient, polly, retry, circuit breaker, resilience, aspnetcore_

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
