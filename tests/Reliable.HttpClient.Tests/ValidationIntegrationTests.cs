using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Reliable.HttpClient.Tests;

public class ValidationIntegrationTests
{
    [Theory]
    [InlineData("TimeoutSeconds", -1, "TimeoutSeconds must be greater than 0*")]
    [InlineData("MaxRetries", -1, "MaxRetries cannot be negative*")]
    [InlineData("FailuresBeforeOpen", 0, "FailuresBeforeOpen must be greater than 0*")]
    public void AddResilience_WithInvalidConfiguration_ShouldThrowOnRegistration(
        string propertyName,
        int invalidValue,
        string expectedMessage)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act & Assert
        services.Invoking(s => s.AddHttpClient("test")
            .AddResilience(options =>
            {
                switch (propertyName)
                {
                    case "TimeoutSeconds":
                        options.TimeoutSeconds = invalidValue;
                        break;
                    case "MaxRetries":
                        options.Retry.MaxRetries = invalidValue;
                        break;
                    case "FailuresBeforeOpen":
                        options.CircuitBreaker.FailuresBeforeOpen = invalidValue;
                        break;
                }
            }))
            .Should().Throw<ArgumentException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void AddResilience_WithValidConfiguration_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act & Assert
        services.Invoking(s => s.AddHttpClient("test")
            .AddResilience(options =>
            {
                options.TimeoutSeconds = 60;
                options.Retry.MaxRetries = 5;
                options.CircuitBreaker.FailuresBeforeOpen = 3;
            }))
            .Should().NotThrow();
    }

    [Fact]
    public void AddResilience_WithDefaultConfiguration_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act & Assert
        services.Invoking(s => s.AddHttpClient("test").AddResilience())
            .Should().NotThrow();
    }
}
