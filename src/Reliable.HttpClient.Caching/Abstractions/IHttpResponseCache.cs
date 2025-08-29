namespace Reliable.HttpClient.Caching.Abstractions;

/// <summary>
/// Defines a cache for HTTP responses
/// </summary>
/// <typeparam name="TResponse">The type of response to cache</typeparam>
public interface IHttpResponseCache<TResponse>
{
    /// <summary>
    /// Gets a cached response by key
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached response or null if not found</returns>
    Task<TResponse?> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a response in the cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="value">Response to cache</param>
    /// <param name="expiry">Optional expiry time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsync(string key, TResponse value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a response from the cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all cached responses
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ClearAsync(CancellationToken cancellationToken = default);
}
