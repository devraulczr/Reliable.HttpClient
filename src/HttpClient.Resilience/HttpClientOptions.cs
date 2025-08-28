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

    /// <summary>
    /// Validates the configuration options
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid</exception>
    public virtual void Validate()
    {
        if (TimeoutSeconds <= 0)
            throw new ArgumentException("TimeoutSeconds must be greater than 0", nameof(TimeoutSeconds));

        if (!string.IsNullOrEmpty(BaseUrl))
        {
            if (!Uri.IsWellFormedUriString(BaseUrl, UriKind.Absolute))
                throw new ArgumentException("BaseUrl must be a valid absolute URI when specified", nameof(BaseUrl));

            var uri = new Uri(BaseUrl);
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                throw new ArgumentException("BaseUrl must use HTTP or HTTPS scheme", nameof(BaseUrl));
        }

        Retry.Validate();
        CircuitBreaker.Validate();
    }
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

    /// <summary>
    /// Validates the retry configuration options
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid</exception>
    public void Validate()
    {
        if (MaxRetries < 0)
            throw new ArgumentException("MaxRetries cannot be negative", nameof(MaxRetries));

        if (BaseDelay <= TimeSpan.Zero)
            throw new ArgumentException("BaseDelay must be greater than zero", nameof(BaseDelay));

        if (MaxDelay <= TimeSpan.Zero)
            throw new ArgumentException("MaxDelay must be greater than zero", nameof(MaxDelay));

        if (BaseDelay > MaxDelay)
            throw new ArgumentException("BaseDelay cannot be greater than MaxDelay", nameof(BaseDelay));

        if (JitterFactor < 0.0 || JitterFactor > 1.0)
            throw new ArgumentException("JitterFactor must be between 0.0 and 1.0", nameof(JitterFactor));
    }
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

    /// <summary>
    /// Validates the circuit breaker configuration options
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid</exception>
    public void Validate()
    {
        if (FailuresBeforeOpen <= 0)
            throw new ArgumentException("FailuresBeforeOpen must be greater than 0", nameof(FailuresBeforeOpen));

        if (OpenDuration <= TimeSpan.Zero)
            throw new ArgumentException("OpenDuration must be greater than zero", nameof(OpenDuration));
    }
}
