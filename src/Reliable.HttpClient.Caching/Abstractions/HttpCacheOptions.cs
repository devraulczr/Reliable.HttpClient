namespace Reliable.HttpClient.Caching.Abstractions;

/// <summary>
/// Configuration options for HTTP response caching
/// </summary>
public class HttpCacheOptions
{
    /// <summary>
    /// Default expiry time for cached responses
    /// </summary>
    public TimeSpan DefaultExpiry { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Maximum number of items to cache (for memory cache)
    /// </summary>
    public int? MaxCacheSize { get; set; } = 1_000;

    /// <summary>
    /// Custom cache key generator
    /// </summary>
    public ICacheKeyGenerator KeyGenerator { get; set; } = new DefaultCacheKeyGenerator();

    /// <summary>
    /// HTTP status codes that should be cached
    /// </summary>
    public HashSet<System.Net.HttpStatusCode> CacheableStatusCodes { get; set; } =
    [
        System.Net.HttpStatusCode.OK,
        System.Net.HttpStatusCode.NotModified,
        System.Net.HttpStatusCode.PartialContent
    ];

    /// <summary>
    /// HTTP methods that should be cached
    /// </summary>
    public HashSet<HttpMethod> CacheableMethods { get; set; } = new()
    {
        HttpMethod.Get,
        HttpMethod.Head
    };

    /// <summary>
    /// Determines if a response should be cached based on the request and response
    /// </summary>
    public Func<HttpRequestMessage, HttpResponseMessage, bool> ShouldCache { get; set; } =
        (request, response) =>
        {
            // Check Cache-Control directives
            if (response.Headers.CacheControl is not null)
            {
                if (response.Headers.CacheControl.NoCache || response.Headers.CacheControl.NoStore)
                    return false;
            }
            return true;
        };

    /// <summary>
    /// Gets the expiry time for a specific request/response pair
    /// </summary>
    public Func<HttpRequestMessage, HttpResponseMessage, TimeSpan> GetExpiry { get; set; } =
        (request, response) =>
        {
            // Check Cache-Control max-age directive
            if (response.Headers.CacheControl?.MaxAge is not null)
            {
                return response.Headers.CacheControl.MaxAge.Value;
            }

            // Check Cache-Control no-cache or no-store directives
            if (response.Headers.CacheControl is not null)
            {
                if (response.Headers.CacheControl.NoCache || response.Headers.CacheControl.NoStore)
                    return TimeSpan.Zero;
            }

            // Fall back to default
            return TimeSpan.FromMinutes(5);
        };
}
