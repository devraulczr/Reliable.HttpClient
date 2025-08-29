namespace Reliable.HttpClient;

/// <summary>
/// Predefined configuration presets for common scenarios
/// </summary>
public static class HttpClientPresets
{
    /// <summary>
    /// Configuration for fast internal APIs (low latency, aggressive retry)
    /// </summary>
    public static HttpClientOptions FastInternalApi() => new HttpClientOptionsBuilder()
        .WithTimeout(TimeSpan.FromSeconds(10))
        .WithRetry(retry => retry
            .WithMaxRetries(5)
            .WithBaseDelay(TimeSpan.FromMilliseconds(200))
            .WithMaxDelay(TimeSpan.FromSeconds(5)))
        .WithCircuitBreaker(cb => cb
            .WithFailureThreshold(3)
            .WithOpenDuration(TimeSpan.FromSeconds(30)))
        .Build();

    /// <summary>
    /// Configuration for slow external APIs (high latency tolerance, conservative retry)
    /// </summary>
    public static HttpClientOptions SlowExternalApi() => new HttpClientOptionsBuilder()
        .WithTimeout(TimeSpan.FromSeconds(120))
        .WithRetry(retry => retry
            .WithMaxRetries(2)
            .WithBaseDelay(TimeSpan.FromSeconds(2))
            .WithMaxDelay(TimeSpan.FromSeconds(30))
            .WithJitter(0.5))
        .WithCircuitBreaker(cb => cb
            .WithFailureThreshold(8)
            .WithOpenDuration(TimeSpan.FromMinutes(5)))
        .Build();

    /// <summary>
    /// Configuration for file downloads (high timeout, no circuit breaker)
    /// </summary>
    public static HttpClientOptions FileDownload() => new HttpClientOptionsBuilder()
        .WithTimeout(TimeSpan.FromMinutes(30))
        .WithRetry(retry => retry
            .WithMaxRetries(3)
            .WithBaseDelay(TimeSpan.FromSeconds(1))
            .WithMaxDelay(TimeSpan.FromSeconds(10)))
        .WithoutCircuitBreaker()
        .Build();

    /// <summary>
    /// Configuration for real-time APIs (minimal retry, fast timeout)
    /// </summary>
    public static HttpClientOptions RealTimeApi() => new HttpClientOptionsBuilder()
        .WithTimeout(TimeSpan.FromSeconds(5))
        .WithRetry(retry => retry
            .WithMaxRetries(1)
            .WithBaseDelay(TimeSpan.FromMilliseconds(100))
            .WithoutJitter())
        .WithCircuitBreaker(cb => cb
            .WithFailureThreshold(10)
            .WithOpenDuration(TimeSpan.FromSeconds(10)))
        .Build();

    /// <summary>
    /// Configuration for authentication APIs (no retry on 401/403)
    /// </summary>
    public static HttpClientOptions AuthenticationApi() => new HttpClientOptionsBuilder()
        .WithTimeout(TimeSpan.FromSeconds(15))
        .WithRetry(retry => retry
            .WithMaxRetries(2)
            .WithBaseDelay(TimeSpan.FromMilliseconds(500)))
        .WithCircuitBreaker(cb => cb
            .WithFailureThreshold(5)
            .WithOpenDuration(TimeSpan.FromMinutes(2)))
        .Build();

    /// <summary>
    /// Configuration for webhooks (minimal settings, no circuit breaker)
    /// </summary>
    public static HttpClientOptions Webhook() => new HttpClientOptionsBuilder()
        .WithTimeout(TimeSpan.FromSeconds(30))
        .WithRetry(retry => retry
            .WithMaxRetries(1)
            .WithBaseDelay(TimeSpan.FromSeconds(1)))
        .WithoutCircuitBreaker()
        .Build();
}
