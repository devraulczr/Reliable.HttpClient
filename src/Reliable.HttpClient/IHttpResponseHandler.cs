namespace Reliable.HttpClient;

/// <summary>
/// Interface for handling HTTP responses from external services
/// </summary>
/// <typeparam name="TResponse">Response type after processing</typeparam>
public interface IHttpResponseHandler<TResponse>
{
    /// <summary>
    /// Processes HTTP response and returns typed result
    /// </summary>
    /// <param name="response">HTTP response to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Processed typed response</returns>
    /// <exception cref="HttpRequestException">On HTTP errors</exception>
    Task<TResponse> HandleAsync(HttpResponseMessage response, CancellationToken cancellationToken = default);
}
