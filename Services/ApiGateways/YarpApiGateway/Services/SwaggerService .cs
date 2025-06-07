using System.Text.Json;
using YarpApiGateway.Configuration;

namespace YarpApiGateway.Services;

public class SwaggerService : ISwaggerService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SwaggerService> _logger;

    public SwaggerService(IHttpClientFactory httpClientFactory, ILogger<SwaggerService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string?> GetFilteredSwaggerJsonAsync(string serviceName)
    {
        var service = ServiceRegistry.GetService(serviceName);
        if (service == null)
        {
            _logger.LogWarning("Service {ServiceName} not found in registry", serviceName);
            return null;
        }

        var httpClient = _httpClientFactory.CreateClient("DefaultClient");
        var swaggerUrl = $"{service.BaseUrl}{service.SwaggerPath}";

        try
        {
            var response = await httpClient.GetAsync(swaggerUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch swagger from {SwaggerUrl}. Status: {StatusCode}",
                    swaggerUrl, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return FilterInternalAPIs(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching swagger for service {ServiceName} from {SwaggerUrl}",
                serviceName, swaggerUrl);
            return null;
        }
    }

    public async Task<Dictionary<string, object>> GetServicesStatusAsync()
    {
        var httpClient = _httpClientFactory.CreateClient("DefaultClient");
        var services = new Dictionary<string, object>();

        var healthCheckTasks = new List<Task<ServiceHealthResult>>();

        foreach (var (serviceName, config) in ServiceRegistry.Services)
        {
            var healthUrl = $"{config.BaseUrl}{config.HealthPath}";
            var task = CheckServiceHealthAsync(httpClient, serviceName, healthUrl);
            healthCheckTasks.Add(task);
        }

        var results = await Task.WhenAll(healthCheckTasks);

        foreach (var result in results)
        {
            services[result.ServiceName] = result.Status;
        }

        return services;
    }

    private async Task<ServiceHealthResult> CheckServiceHealthAsync(
        HttpClient httpClient,
        string serviceName,
        string healthUrl)
    {
        try
        {
            var response = await httpClient.GetAsync(healthUrl);
            var status = new
            {
                Status = response.IsSuccessStatusCode ? "Healthy" : "Unhealthy",
                StatusCode = (int)response.StatusCode,
                ResponseTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Url = healthUrl
            };
            return new ServiceHealthResult(serviceName, status);
        }
        catch (HttpRequestException ex)
        {
            var status = new
            {
                Status = "Unreachable",
                Error = ex.Message,
                Url = healthUrl
            };
            return new ServiceHealthResult(serviceName, status);
        }
        catch (TaskCanceledException)
        {
            var status = new
            {
                Status = "Timeout",
                Error = "Service did not respond within timeout period",
                Url = healthUrl
            };
            return new ServiceHealthResult(serviceName, status);
        }
    }

    private string FilterInternalAPIs(string swaggerJson)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(swaggerJson);
            var options = new JsonWriterOptions { Indented = false };

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, options))
            {
                writer.WriteStartObject();

                foreach (var property in jsonDoc.RootElement.EnumerateObject())
                {
                    if (property.Name == "paths")
                    {
                        WritePaths(writer, property.Value);
                    }
                    else
                    {
                        writer.WritePropertyName(property.Name);
                        property.Value.WriteTo(writer);
                    }
                }

                writer.WriteEndObject();
            }

            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to filter swagger JSON, returning original");
            return swaggerJson;
        }
    }

    private void WritePaths(Utf8JsonWriter writer, JsonElement pathsElement)
    {
        writer.WritePropertyName("paths");
        writer.WriteStartObject();

        foreach (var pathProperty in pathsElement.EnumerateObject())
        {
            var pathName = pathProperty.Name;

            if (!IsInternalPath(pathName))
            {
                writer.WritePropertyName(pathName);
                pathProperty.Value.WriteTo(writer);
            }
            else
            {
                _logger.LogDebug("Filtered internal path: {Path}", pathName);
            }
        }

        writer.WriteEndObject();
    }

    private static bool IsInternalPath(string path)
    {
        var internalPatterns = new[]
        {
            "/api/internal/",
            "/internal/",
            "/admin/internal/"
        };

        return internalPatterns.Any(pattern =>
            path.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}

// Helper record for type safety
public record ServiceHealthResult(string ServiceName, object Status);