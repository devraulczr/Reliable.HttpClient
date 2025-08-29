namespace Reliable.HttpClient;

/// <summary>
/// Builder for fluent configuration of HTTP client options
/// </summary>
public class HttpClientOptionsBuilder
{
    private readonly HttpClientOptions _options = new();

    /// <summary>
    /// Sets the base URL for HTTP requests
    /// </summary>
    /// <param name="baseUrl">Base URL</param>
    /// <returns>Builder for method chaining</returns>
    public HttpClientOptionsBuilder WithBaseUrl(string baseUrl)
    {
        _options.BaseUrl = baseUrl;
        return this;
    }

    /// <summary>
    /// Sets the request timeout
    /// </summary>
    /// <param name="timeout">Timeout duration</param>
    /// <returns>Builder for method chaining</returns>
    public HttpClientOptionsBuilder WithTimeout(TimeSpan timeout)
    {
        _options.TimeoutSeconds = (int)timeout.TotalSeconds;
        return this;
    }

    /// <summary>
    /// Sets the User-Agent header
    /// </summary>
    /// <param name="userAgent">User agent string</param>
    /// <returns>Builder for method chaining</returns>
    public HttpClientOptionsBuilder WithUserAgent(string userAgent)
    {
        _options.UserAgent = userAgent;
        return this;
    }

    /// <summary>
    /// Configures retry policy
    /// </summary>
    /// <param name="configure">Retry configuration action</param>
    /// <returns>Builder for method chaining</returns>
    public HttpClientOptionsBuilder WithRetry(Action<RetryOptionsBuilder> configure)
    {
        var builder = new RetryOptionsBuilder(_options.Retry);
        configure(builder);
        return this;
    }

    /// <summary>
    /// Configures circuit breaker policy
    /// </summary>
    /// <param name="configure">Circuit breaker configuration action</param>
    /// <returns>Builder for method chaining</returns>
    public HttpClientOptionsBuilder WithCircuitBreaker(Action<CircuitBreakerOptionsBuilder> configure)
    {
        var builder = new CircuitBreakerOptionsBuilder(_options.CircuitBreaker);
        configure(builder);
        return this;
    }

    /// <summary>
    /// Disables circuit breaker
    /// </summary>
    /// <returns>Builder for method chaining</returns>
    public HttpClientOptionsBuilder WithoutCircuitBreaker()
    {
        _options.CircuitBreaker.Enabled = false;
        return this;
    }

    /// <summary>
    /// Builds the HTTP client options
    /// </summary>
    /// <returns>Configured options</returns>
    public HttpClientOptions Build()
    {
        _options.Validate();
        return _options;
    }

    /// <summary>
    /// Implicitly converts builder to options
    /// </summary>
    public static implicit operator HttpClientOptions(HttpClientOptionsBuilder builder) => builder.Build();
}

/// <summary>
/// Builder for retry options
/// </summary>
public class RetryOptionsBuilder
{
    private readonly RetryOptions _options;

    internal RetryOptionsBuilder(RetryOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Sets maximum number of retries
    /// </summary>
    /// <param name="maxRetries">Maximum retry count</param>
    /// <returns>Builder for method chaining</returns>
    public RetryOptionsBuilder WithMaxRetries(int maxRetries)
    {
        _options.MaxRetries = maxRetries;
        return this;
    }

    /// <summary>
    /// Sets base delay between retries
    /// </summary>
    /// <param name="delay">Base delay</param>
    /// <returns>Builder for method chaining</returns>
    public RetryOptionsBuilder WithBaseDelay(TimeSpan delay)
    {
        _options.BaseDelay = delay;
        return this;
    }

    /// <summary>
    /// Sets maximum delay between retries
    /// </summary>
    /// <param name="delay">Maximum delay</param>
    /// <returns>Builder for method chaining</returns>
    public RetryOptionsBuilder WithMaxDelay(TimeSpan delay)
    {
        _options.MaxDelay = delay;
        return this;
    }

    /// <summary>
    /// Sets jitter factor for randomizing delays
    /// </summary>
    /// <param name="factor">Jitter factor (0.0 to 1.0)</param>
    /// <returns>Builder for method chaining</returns>
    public RetryOptionsBuilder WithJitter(double factor)
    {
        _options.JitterFactor = factor;
        return this;
    }

    /// <summary>
    /// Disables jitter (sets factor to 0)
    /// </summary>
    /// <returns>Builder for method chaining</returns>
    public RetryOptionsBuilder WithoutJitter()
    {
        _options.JitterFactor = 0.0;
        return this;
    }
}

/// <summary>
/// Builder for circuit breaker options
/// </summary>
public class CircuitBreakerOptionsBuilder
{
    private readonly CircuitBreakerOptions _options;

    internal CircuitBreakerOptionsBuilder(CircuitBreakerOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Sets failure threshold before opening circuit
    /// </summary>
    /// <param name="failures">Number of failures</param>
    /// <returns>Builder for method chaining</returns>
    public CircuitBreakerOptionsBuilder WithFailureThreshold(int failures)
    {
        _options.FailuresBeforeOpen = failures;
        return this;
    }

    /// <summary>
    /// Sets duration to keep circuit open
    /// </summary>
    /// <param name="duration">Open duration</param>
    /// <returns>Builder for method chaining</returns>
    public CircuitBreakerOptionsBuilder WithOpenDuration(TimeSpan duration)
    {
        _options.OpenDuration = duration;
        return this;
    }
}
