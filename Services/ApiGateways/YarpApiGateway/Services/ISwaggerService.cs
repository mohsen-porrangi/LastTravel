using System.Text.Json;
using YarpApiGateway.Configuration;

namespace YarpApiGateway.Services;

public interface ISwaggerService
{
    Task<string?> GetFilteredSwaggerJsonAsync(string serviceName);
    Task<Dictionary<string, object>> GetServicesStatusAsync();
}