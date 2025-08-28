using System.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

using HttpClientType = System.Net.Http.HttpClient;

namespace HttpClient.Resilience;

/// <summary>
/// Extension methods for registering HTTP clients with resilience policies
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Configures HTTP client with resilience policies
    /// </summary>
    /// <param name="builder">HTTP client builder</param>
    /// <param name="configureOptions">Action to configure resilience options</param>
    /// <returns>HTTP client builder for method chaining</returns>
    public static IHttpClientBuilder AddResilience(
        this IHttpClientBuilder builder,
        Action<HttpClientOptions>? configureOptions = null)
    {
        var options = new HttpClientOptions();
        configureOptions?.Invoke(options);

        return builder
            .AddPolicyHandler(CreateRetryPolicy(options))
            .AddPolicyHandler(CreateCircuitBreakerPolicy(options));
    }

    /// <summary>
    /// Configures HTTP client with basic configuration and resilience policies
    /// </summary>
    /// <typeparam name="TClient">HTTP client type</typeparam>
    /// <typeparam name="TOptions">Configuration options type</typeparam>
    /// <param name="builder">HTTP client builder</param>
    /// <param name="configureClient">Additional HTTP client configuration</param>
    /// <returns>HTTP client builder for method chaining</returns>
    public static IHttpClientBuilder ConfigureResilientClient<TClient, TOptions>(
        this IHttpClientBuilder builder,
        Action<TOptions, HttpClientType>? configureClient = null)
        where TClient : class
        where TOptions : HttpClientOptions
    {
        return builder
            .ConfigureHttpClient((serviceProvider, client) =>
                ConfigureHttpClientCore(serviceProvider, client, configureClient))
            .AddPolicyHandler((serviceProvider, request) => CreateRetryPolicy<TClient, TOptions>(serviceProvider))
            .AddPolicyHandler((serviceProvider, request) => CreateCircuitBreakerPolicy<TClient, TOptions>(serviceProvider));
    }

    /// <summary>
    /// Configures HTTP client with basic configuration and resilience policies (with named logger)
    /// </summary>
    /// <typeparam name="TOptions">Configuration options type</typeparam>
    /// <param name="builder">HTTP client builder</param>
    /// <param name="loggerName">Logger name</param>
    /// <param name="configureClient">Additional HTTP client configuration</param>
    /// <returns>HTTP client builder for method chaining</returns>
    public static IHttpClientBuilder ConfigureResilientClient<TOptions>(
        this IHttpClientBuilder builder,
        string loggerName,
        Action<TOptions, HttpClientType>? configureClient = null)
        where TOptions : HttpClientOptions
    {
        return builder
            .ConfigureHttpClient((serviceProvider, client) =>
                ConfigureHttpClientCore(serviceProvider, client, configureClient))
            .AddPolicyHandler((serviceProvider, request) => CreateRetryPolicyNamed<TOptions>(serviceProvider, loggerName))
            .AddPolicyHandler((serviceProvider, request) => CreateCircuitBreakerPolicyNamed<TOptions>(serviceProvider, loggerName));
    }

    /// <summary>
    /// Basic HTTP client configuration based on options
    /// </summary>
    private static void ConfigureHttpClientCore<TOptions>(
        IServiceProvider serviceProvider,
        HttpClientType client,
        Action<TOptions, HttpClientType>? configureClient)
        where TOptions : HttpClientOptions
    {
        TOptions options = serviceProvider.GetRequiredService<IOptions<TOptions>>().Value;

        if (!string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            client.BaseAddress = new Uri(options.BaseUrl);
        }

        client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

        if (!string.IsNullOrWhiteSpace(options.UserAgent))
        {
            client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
        }

        configureClient?.Invoke(options, client);
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(HttpClientOptions options)
    {
        var random = new Random();

        return Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(msg =>
                (int)msg.StatusCode >= 500 ||
                msg.StatusCode == HttpStatusCode.RequestTimeout ||
                msg.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: options.Retry.MaxRetries,
                sleepDurationProvider: retryAttempt =>
                {
                    var delay = TimeSpan.FromMilliseconds(
                        options.Retry.BaseDelay.TotalMilliseconds * Math.Pow(2, retryAttempt - 1));

                    TimeSpan finalDelay = delay > options.Retry.MaxDelay ? options.Retry.MaxDelay : delay;

                    // Add jitter (random deviation) to avoid thundering herd
                    var jitterRange = finalDelay.TotalMilliseconds * options.Retry.JitterFactor;
                    var jitter = random.NextDouble() * jitterRange * 2 - jitterRange; // ±jitterRange
                    var finalDelayMs = Math.Max(0, finalDelay.TotalMilliseconds + jitter);

                    return TimeSpan.FromMilliseconds(finalDelayMs);
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy(HttpClientOptions options)
    {
        return Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(msg =>
                (int)msg.StatusCode >= 500 ||
                msg.StatusCode == HttpStatusCode.RequestTimeout ||
                msg.StatusCode == HttpStatusCode.TooManyRequests)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: options.CircuitBreaker.FailuresBeforeOpen,
                durationOfBreak: options.CircuitBreaker.OpenDuration);
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy<TClient, TOptions>(IServiceProvider serviceProvider)
        where TClient : class
        where TOptions : HttpClientOptions
    {
        TOptions options = serviceProvider.GetRequiredService<IOptions<TOptions>>().Value;
        ILogger<TClient> logger = serviceProvider.GetRequiredService<ILogger<TClient>>();

        return CreateRetryPolicyCore(options, logger, typeof(TClient).Name);
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicyNamed<TOptions>(
        IServiceProvider serviceProvider, string loggerName)
        where TOptions : HttpClientOptions
    {
        TOptions options = serviceProvider.GetRequiredService<IOptions<TOptions>>().Value;
        ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        ILogger logger = loggerFactory.CreateLogger(loggerName);

        return CreateRetryPolicyCore(options, logger, loggerName);
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicyCore(
        HttpClientOptions options,
        ILogger logger,
        string clientName)
    {
        var random = new Random();

        return Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(msg =>
                (int)msg.StatusCode >= 500 ||
                msg.StatusCode == HttpStatusCode.RequestTimeout ||
                msg.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: options.Retry.MaxRetries,
                sleepDurationProvider: retryAttempt =>
                {
                    var delay = TimeSpan.FromMilliseconds(
                        options.Retry.BaseDelay.TotalMilliseconds * Math.Pow(2, retryAttempt - 1));

                    TimeSpan finalDelay = delay > options.Retry.MaxDelay ? options.Retry.MaxDelay : delay;

                    // Add jitter (random deviation) to avoid thundering herd
                    var jitterRange = finalDelay.TotalMilliseconds * options.Retry.JitterFactor;
                    var jitter = random.NextDouble() * jitterRange * 2 - jitterRange;
                    var finalDelayMs = Math.Max(0, finalDelay.TotalMilliseconds + jitter);

                    return TimeSpan.FromMilliseconds(finalDelayMs);
                },
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    var message = outcome.Exception is not null
                        ? $"Retry {retryCount} after exception: {outcome.Exception.Message}"
                        : $"Retry {retryCount} after HTTP {(int)outcome.Result.StatusCode}: {outcome.Result.ReasonPhrase}";

                    logger.LogWarning("{ClientName} HTTP retry. {Message}. Delay: {Delay}ms",
                        clientName, message, timespan.TotalMilliseconds);
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy<TClient, TOptions>(
        IServiceProvider serviceProvider)
        where TClient : class
        where TOptions : HttpClientOptions
    {
        TOptions options = serviceProvider.GetRequiredService<IOptions<TOptions>>().Value;
        ILogger<TClient> logger = serviceProvider.GetRequiredService<ILogger<TClient>>();

        return CreateCircuitBreakerPolicyCore(options, logger, typeof(TClient).Name);
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicyNamed<TOptions>(
        IServiceProvider serviceProvider, string loggerName)
        where TOptions : HttpClientOptions
    {
        TOptions options = serviceProvider.GetRequiredService<IOptions<TOptions>>().Value;
        ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        ILogger logger = loggerFactory.CreateLogger(loggerName);

        return CreateCircuitBreakerPolicyCore(options, logger, loggerName);
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicyCore(
        HttpClientOptions options,
        ILogger logger,
        string clientName)
    {
        if (!options.CircuitBreaker.Enabled)
        {
            return Policy.NoOpAsync<HttpResponseMessage>();
        }

        return Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(msg =>
                (int)msg.StatusCode >= 500 ||
                msg.StatusCode == HttpStatusCode.RequestTimeout ||
                msg.StatusCode == HttpStatusCode.TooManyRequests)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: options.CircuitBreaker.FailuresBeforeOpen,
                durationOfBreak: options.CircuitBreaker.OpenDuration,
                onBreak: (result, timespan) =>
                {
                    var errorMessage = result.Exception?.Message ??
                        (result.Result != null ? $"HTTP {(int)result.Result.StatusCode}: {result.Result.ReasonPhrase}" : "Unknown error");

                    logger.LogError("{ClientName} HTTP circuit breaker opened for {Duration}ms due to: {Error}",
                        clientName, timespan.TotalMilliseconds, errorMessage);
                },
                onReset: () =>
                {
                    logger.LogInformation("{ClientName} HTTP circuit breaker reset", clientName);
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("{ClientName} HTTP circuit breaker in half-open state", clientName);
                });
    }
}
