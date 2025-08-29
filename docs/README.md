# Reliable.HttpClient Documentation

Welcome to the comprehensive documentation for Reliable.HttpClient - a lightweight resilience library for HttpClient.

## Table of Contents

### Getting Started

- [Quick Start Guide](getting-started.md) - Get up and running in minutes
- [Installation & Setup](getting-started.md#installation) - Package installation and basic configuration
- [First Steps](getting-started.md#basic-setup) - Your first resilient HttpClient

### Configuration

- [Configuration Reference](configuration.md) - Complete options reference
- [Retry Policies](configuration.md#retry-configuration) - Configuring retry behavior
- [Circuit Breakers](configuration.md#circuit-breaker-configuration) - Preventing cascading failures
- [Validation Rules](configuration.md#validation) - Understanding configuration validation

### Usage Patterns

- [Advanced Usage](advanced-usage.md) - Advanced patterns and techniques
- [Multiple Clients](advanced-usage.md#multiple-named-httpclients) - Managing different service configurations
- [Typed Clients](advanced-usage.md#typed-httpclients) - Using with typed HttpClient pattern
- [Custom Error Handling](advanced-usage.md#custom-error-handling) - Implementing custom retry logic
- [Testing Strategies](advanced-usage.md#testing-with-resilience) - Unit and integration testing approaches

### Examples

- [Common Scenarios](examples/common-scenarios.md) - Real-world usage examples
- [E-commerce Integration](examples/common-scenarios.md#scenario-1-e-commerce-api-integration) - Payment and inventory APIs
- [Microservices Communication](examples/common-scenarios.md#scenario-2-microservices-communication) - Service-to-service patterns
- [External APIs](examples/common-scenarios.md#scenario-3-external-api-with-rate-limiting) - Handling rate limits and quotas
- [Legacy Systems](examples/common-scenarios.md#scenario-4-legacy-system-integration) - Working with unreliable systems

### Reference

- [Default Values](configuration.md#overview) - All default configuration values
- [Error Codes](configuration.md#validation) - Understanding validation errors

### Contributing

- [Contributing Guide](../CONTRIBUTING.md) - How to contribute to the project
- [Development Setup](../CONTRIBUTING.md#getting-started) - Setting up the development environment
- [Code Style](../CONTRIBUTING.md#code-style) - Coding standards and conventions
- [Testing Guidelines](../CONTRIBUTING.md#writing-tests) - How to write effective tests

## Quick Reference

### Minimal Setup

```csharp
builder.Services.AddHttpClient("api")
    .AddResilience();
```

### Custom Configuration

```csharp
builder.Services.AddHttpClient("api")
    .AddResilience(options =>
    {
        options.Retry.MaxRetries = 5;
        options.CircuitBreaker.FailuresBeforeOpen = 10;
    });
```

### Environment-Specific

```csharp
#if DEBUG
    .AddResilience(options => options.Retry.MaxRetries = 1);
#else
    .AddResilience(); // Production defaults
#endif
```

## Key Concepts

### Retry Policies

Automatically retry failed requests with exponential backoff and jitter to handle transient failures gracefully.

### Circuit Breakers

Prevent cascading failures by temporarily stopping requests to failing services and allowing them time to recover.

### Configuration Validation

All settings are validated at startup to catch configuration errors early and provide clear error messages.

### Zero Configuration

Works out of the box with production-ready defaults, but allows customization when needed.

## Support

- 📖 [Documentation](README.md) – This documentation site
- 🐛 [Issues](https://github.com/akrisanov/Reliable.HttpClient/issues) – Bug reports and feature requests
- 💬 [Discussions](https://github.com/akrisanov/Reliable.HttpClient/discussions) – Community discussions

---

> **Note**: This documentation is for the latest version. Check the version badge in the main README for compatibility.
