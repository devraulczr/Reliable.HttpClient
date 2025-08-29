using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Reliable.HttpClient.Caching.Abstractions;
using Reliable.HttpClient.Caching.Extensions;
using Xunit;

namespace Reliable.HttpClient.Caching.Tests;

public class CachePresetsIntegrationTests
{
    [Fact]
    public void AddMediumTermCache_ShouldUseTenMinutesExpiry()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMemoryCache();

        // Act
        services.AddHttpClient<TestClient>()
            .AddMediumTermCache<string>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Get the configured options for the named client
        IOptionsSnapshot<HttpCacheOptions> optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<HttpCacheOptions>>();
        HttpCacheOptions options = optionsSnapshot.Get("TestClient");

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(10), options.DefaultExpiry);

        // Test GetExpiry function with a mock response
        var request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        TimeSpan expiry = options.GetExpiry(request, response);
        Assert.Equal(TimeSpan.FromMinutes(10), expiry);
    }

    [Fact]
    public void AddShortTermCache_ShouldUseOneMinuteExpiry()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMemoryCache();

        // Act
        services.AddHttpClient<TestClient>()
            .AddShortTermCache<string>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Get the configured options for the named client
        IOptionsSnapshot<HttpCacheOptions> optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<HttpCacheOptions>>();
        HttpCacheOptions options = optionsSnapshot.Get("TestClient");

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(1), options.DefaultExpiry);

        // Test GetExpiry function with a mock response
        var request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        TimeSpan expiry = options.GetExpiry(request, response);
        Assert.Equal(TimeSpan.FromMinutes(1), expiry);
    }

    [Fact]
    public void AddHighPerformanceCache_ShouldUseFiveMinutesExpiry()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMemoryCache();

        // Act
        services.AddHttpClient<TestClient>()
            .AddHighPerformanceCache<string>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Get the configured options for the named client
        IOptionsSnapshot<HttpCacheOptions> optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<HttpCacheOptions>>();
        HttpCacheOptions options = optionsSnapshot.Get("TestClient");

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(5), options.DefaultExpiry);

        // Test GetExpiry function with a mock response
        var request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        TimeSpan expiry = options.GetExpiry(request, response);
        Assert.Equal(TimeSpan.FromMinutes(5), expiry);
    }

    private class TestClient
    {
    }
}
