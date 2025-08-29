using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Reliable.HttpClient.Caching.Abstractions;
using Reliable.HttpClient.Caching.Providers;

namespace Reliable.HttpClient.Caching.Extensions;

/// <summary>
/// Extension methods for adding HTTP caching to HttpClient
/// </summary>
public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Adds memory caching to HttpClient
    /// </summary>
    /// <typeparam name="TResponse">Response type to cache</typeparam>
    /// <param name="builder">HttpClient builder</param>
    /// <param name="configureOptions">Configure cache options</param>
    /// <returns>HttpClient builder for chaining</returns>
    public static IHttpClientBuilder AddMemoryCache<TResponse>(
        this IHttpClientBuilder builder,
        Action<HttpCacheOptions>? configureOptions = null)
    {
        // Register cache options using Options pattern with client name
        if (configureOptions is not null)
        {
            builder.Services.Configure(builder.Name, configureOptions);
        }

        // Register memory cache if not already registered
        builder.Services.TryAddSingleton<IMemoryCache, MemoryCache>();

        // Register cache provider as scoped (one per request/scope)
        builder.Services.TryAddScoped<IHttpResponseCache<TResponse>, MemoryCacheProvider<TResponse>>();

        // Register cached HTTP client as scoped
        builder.Services.TryAddScoped<CachedHttpClient<TResponse>>();

        return builder;
    }

    /// <summary>
    /// Adds custom cache provider to HttpClient
    /// </summary>
    /// <typeparam name="TResponse">Response type to cache</typeparam>
    /// <typeparam name="TCacheProvider">Cache provider type</typeparam>
    /// <param name="builder">HttpClient builder</param>
    /// <param name="configureOptions">Configure cache options</param>
    /// <returns>HttpClient builder for chaining</returns>
    public static IHttpClientBuilder AddCache<TResponse, TCacheProvider>(
        this IHttpClientBuilder builder,
        Action<HttpCacheOptions>? configureOptions = null)
        where TCacheProvider : class, IHttpResponseCache<TResponse>
    {
        // Register cache options using Options pattern with client name
        if (configureOptions is not null)
        {
            builder.Services.Configure(builder.Name, configureOptions);
        }

        // Register custom cache provider as scoped
        builder.Services.TryAddScoped<IHttpResponseCache<TResponse>, TCacheProvider>();

        // Register cached HTTP client as scoped
        builder.Services.TryAddScoped<CachedHttpClient<TResponse>>();

        return builder;
    }
}

/// <summary>
/// Extension methods for ServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds HTTP caching services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Configure default cache options</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddHttpCaching(
        this IServiceCollection services,
        Action<HttpCacheOptions>? configureOptions = null)
    {
        // Register default cache options
        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }

        // Register memory cache if not already registered
        services.TryAddSingleton<IMemoryCache, MemoryCache>();

        return services;
    }

    /// <summary>
    /// Adds HTTP client caching for a specific HTTP client and response type
    /// </summary>
    /// <typeparam name="TClient">HTTP client type</typeparam>
    /// <typeparam name="TResponse">Response type to cache</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Configure cache options</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddHttpClientCaching<TClient, TResponse>(
        this IServiceCollection services,
        Action<HttpCacheOptions>? configureOptions = null)
        where TClient : class
    {
        // Check if IMemoryCache is registered
        var hasMemoryCache = services.Any(x => x.ServiceType == typeof(IMemoryCache));
        if (!hasMemoryCache)
        {
            throw new InvalidOperationException(
                "IMemoryCache is not registered. Please call services.AddMemoryCache() or services.AddHttpCaching() first.");
        }

        // Register default cache options
        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }

        // Register cache key generator as singleton
        services.TryAddSingleton<ICacheKeyGenerator, DefaultCacheKeyGenerator>();

        // Register cache provider as scoped (one per request/scope)
        services.TryAddScoped<IHttpResponseCache<TResponse>, MemoryCacheProvider<TResponse>>();

        // Register cached HTTP client as scoped
        services.TryAddScoped<CachedHttpClient<TResponse>>();

        return services;
    }
}
