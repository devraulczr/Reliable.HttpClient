namespace HttpClient.Resilience;

/// <summary>
/// Base settings for HTTP clients with resilience policies
/// </summary>
public class HttpClientOptions
{
    /// <summary>
    /// Base API URL
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// User-Agent for HTTP requests
    /// </summary>
    public string UserAgent { get; set; } = "HttpClient.Resilience/1.0";

    /// <summary>
    /// Retry policy configuration
    /// </summary>
    public RetryOptions Retry { get; set; } = new();

    /// <summary>
    /// Circuit breaker policy configuration
    /// </summary>
    public CircuitBreakerOptions CircuitBreaker { get; set; } = new();
}

/// <summary>
/// Retry policy configuration options
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// Maximum number of retry attempts on error
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Base delay before retry
    /// </summary>
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromMilliseconds(1_000);

    /// <summary>
    /// Maximum delay before retry
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMilliseconds(30_000);

    /// <summary>
    /// Jitter factor for randomizing retry delays (0.0 to 1.0)
    /// </summary>
    public double JitterFactor { get; set; } = 0.25;
}

/// <summary>
/// Circuit breaker policy configuration options
/// </summary>
public class CircuitBreakerOptions
{
    /// <summary>
    /// Enable Circuit Breaker policy
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Number of failures before opening Circuit Breaker
    /// </summary>
    public int FailuresBeforeOpen { get; set; } = 5;

    /// <summary>
    /// Circuit Breaker open duration
    /// </summary>
    public TimeSpan OpenDuration { get; set; } = TimeSpan.FromMilliseconds(60_000);
}
