using Microsoft.Extensions.Logging;
using Polly; //TODO review this packages
using Polly.Extensions.Http;
using System.Net.Http.Json;
using System.Text.Json;
//TODO review using HttpClient in this layer
namespace Order.Infrastructure.ExternalServices.Common;

public abstract class BaseHttpClient(HttpClient httpClient, ILogger logger)
{
    protected readonly HttpClient HttpClient = httpClient;
    protected readonly ILogger Logger = logger;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await GetRetryPolicy()
                .ExecuteAsync(async () => await httpClient.GetAsync(endpoint, cancellationToken));

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling GET {Endpoint}", endpoint);
            throw;
        }
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await GetRetryPolicy()
                .ExecuteAsync(async () => await httpClient.PostAsJsonAsync(endpoint, request, JsonOptions, cancellationToken));

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling POST {Endpoint}", endpoint);
            throw;
        }
    }
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
