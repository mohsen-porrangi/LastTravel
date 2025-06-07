using YarpApiGateway.Configuration;
using YarpApiGateway.Services;

namespace YarpApiGateway.Endpoints;

public static class GatewayEndpoints
{
    public static void MapGatewayEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthEndpoints();
        app.MapSwaggerProxyEndpoints();
        app.MapInformationEndpoints();
        app.MapMonitoringEndpoints();

        if (app.ServiceProvider.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            app.MapTestEndpoints();
            app.MapDebugEndpoints();
        }
    }

    private static void MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        var healthGroup = app.MapGroup("/health")
            .WithTags("Health");

        healthGroup.MapGet("", () => Results.Ok(new
        {
            Status = "Healthy",
            Service = "API Gateway",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        }))
        .WithName("HealthCheck")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Gateway Health Check";
            operation.Description = "Check if the API Gateway is running properly";
            return operation;
        });
    }

    private static void MapSwaggerProxyEndpoints(this IEndpointRouteBuilder app)
    {
        var swaggerGroup = app.MapGroup("/swagger/external")
            .WithTags("Swagger");

        swaggerGroup.MapGet("/{service}/swagger.json", async (
            string service,
            ISwaggerService swaggerService) =>
        {
            var content = await swaggerService.GetFilteredSwaggerJsonAsync(service);

            if (content == null)
            {
                return Results.NotFound(new { error = $"Service '{service}' not found or swagger not available" });
            }

            return Results.Content(content, "application/json");
        })
        .WithName("SwaggerProxy")
        .AllowAnonymous()
        .WithOpenApi(operation =>
        {
            operation.Summary = "Swagger JSON Proxy";
            operation.Description = "Proxy filtered swagger.json files from backend services";
            return operation;
        });
    }

    private static void MapInformationEndpoints(this IEndpointRouteBuilder app)
    {
        var infoGroup = app.MapGroup("/gateway")
            .WithTags("Gateway");

        infoGroup.MapGet("/info", () => Results.Ok(new
        {
            Name = "آفاق سیر API Gateway",
            Version = "1.0.0",
            Environment = app.ServiceProvider.GetRequiredService<IWebHostEnvironment>().EnvironmentName,
            Uptime = DateTime.UtcNow,
            Features = new[]
            {
                "Rate Limiting",
                "CORS Support",
                "Internal API Protection",
                "Service Health Monitoring",
                "Request Routing",
                "Swagger Proxy",
                "API Filtering"
            },
            SupportedServices = ServiceRegistry.Services.Values.Select(s => s.DisplayName).ToArray()
        }))
        .WithName("GatewayInfo")
        .WithOpenApi();

        infoGroup.MapGet("/routes", () => Results.Ok(new
        {
            Description = "Available routes through this gateway",
            Services = ServiceRegistry.Services.ToDictionary(
                kvp => kvp.Key,
                kvp => new
                {
                    kvp.Value.DisplayName,
                    kvp.Value.BaseUrl,
                    kvp.Value.Endpoints
                })
        }))
        .WithName("GatewayRoutes")
        .WithOpenApi();

        infoGroup.MapGet("/services/status", async (ISwaggerService swaggerService) =>
        {
            var services = await swaggerService.GetServicesStatusAsync();

            return Results.Ok(new
            {
                Gateway = "Healthy",
                Services = services,
                Timestamp = DateTime.UtcNow
            });
        })
        .WithName("ServicesStatus")
        .WithOpenApi();
    }

    private static void MapMonitoringEndpoints(this IEndpointRouteBuilder app)
    {
        var monitorGroup = app.MapGroup("/monitor")
            .WithTags("Monitoring");

        monitorGroup.MapGet("/dashboard", () =>
        {
            var servicesLinks = string.Join("", ServiceRegistry.Services.Select(kvp =>
                $"<div>{kvp.Value.Icon} {kvp.Value.DisplayName} - <a href='{kvp.Value.BaseUrl}/swagger' target='_blank'>View</a></div>"));

            var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>Gateway Dashboard</title>
                <meta http-equiv='refresh' content='30'>
                <style>
                    body {{ font-family: Arial; margin: 20px; background: #f5f5f5; }}
                    .card {{ background: white; padding: 20px; margin: 10px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                    .status-healthy {{ color: #16a34a; }}
                    .status-unhealthy {{ color: #dc2626; }}
                    .timestamp {{ color: #6b7280; font-size: 12px; }}
                    h1 {{ color: #1e3a8a; }}
                    .grid {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 20px; }}
                </style>
            </head>
            <body>
                <h1>🌐 آفاق سیر API Gateway Dashboard</h1>
                <div class='timestamp'>Last Updated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</div>
                
                <div class='grid'>
                    <div class='card'>
                        <h3>📊 Gateway Status</h3>
                        <div class='status-healthy'>✅ Healthy</div>
                        <p>Uptime: {DateTime.UtcNow}</p>
                    </div>
                    
                    <div class='card'>
                        <h3>🔗 Quick Links</h3>
                        <ul>
                            <li><a href='/swagger' target='_blank'>📚 Swagger UI</a></li>
                            <li><a href='/gateway/services/status' target='_blank'>🩺 Services Health</a></li>
                            <li><a href='/debug/swagger-status' target='_blank'>🔍 Debug Info</a></li>
                        </ul>
                    </div>
                    
                    <div class='card'>
                        <h3>⚡ Services</h3>
                        {servicesLinks}
                    </div>
                    
                    <div class='card'>
                        <h3>🛠️ Development Tips</h3>
                        <ul>
                            <li>Use <strong>Ctrl+F5</strong> for hard refresh</li>
                            <li>Check browser console for errors</li>
                            <li>Clear cache if services don't appear</li>
                        </ul>
                    </div>
                </div>
                
                <script>
                    setInterval(() => {{
                        fetch('/gateway/services/status')
                            .then(r => r.json())
                            .then(data => {{
                                console.log('Services status updated:', data);
                            }});
                    }}, 30000);
                </script>
            </body>
            </html>";

            return Results.Content(html, "text/html");
        })
        .WithName("Dashboard")
        .WithOpenApi();
    }

    private static void MapTestEndpoints(this IEndpointRouteBuilder app)
    {
        var testGroup = app.MapGroup("/test")
            .WithTags("Testing")
            .ExcludeFromDescription();

        testGroup.MapGet("/services", async (IHttpClientFactory httpClientFactory) =>
        {
            var httpClient = httpClientFactory.CreateClient("DefaultClient");
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            var results = new Dictionary<string, object>();

            foreach (var (serviceName, config) in ServiceRegistry.Services)
            {
                try
                {
                    var swaggerUrl = $"{config.BaseUrl}{config.SwaggerPath}";
                    var response = await httpClient.GetAsync(swaggerUrl);

                    results[serviceName] = new
                    {
                        Available = response.IsSuccessStatusCode,
                        StatusCode = (int)response.StatusCode,
                        SwaggerUrl = swaggerUrl,
                        config.DisplayName
                    };
                }
                catch (Exception ex)
                {
                    results[serviceName] = new
                    {
                        Available = false,
                        Error = ex.Message,
                        config.DisplayName
                    };
                }
            }

            return Results.Ok(results);
        })
        .WithName("TestServices");

        testGroup.MapGet("/proxy/{service}/{**path}", async (
            string service,
            string path,
            IHttpClientFactory httpClientFactory) =>
        {
            var serviceConfig = ServiceRegistry.GetService(service);
            if (serviceConfig == null)
            {
                return Results.BadRequest(new
                {
                    error = $"Invalid service. Available services: {string.Join(", ", ServiceRegistry.Services.Keys)}"
                });
            }

            var httpClient = httpClientFactory.CreateClient("DefaultClient");

            try
            {
                var targetUrl = $"{serviceConfig.BaseUrl}/{path}";
                var response = await httpClient.GetAsync(targetUrl);
                var content = await response.Content.ReadAsStringAsync();

                return Results.Ok(new
                {
                    success = response.IsSuccessStatusCode,
                    statusCode = (int)response.StatusCode,
                    targetUrl,
                    content = response.IsSuccessStatusCode ? content : $"Error: {content}"
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("TestProxyManual");
    }

    private static void MapDebugEndpoints(this IEndpointRouteBuilder app)
    {
        var debugGroup = app.MapGroup("/debug")
            .WithTags("Debug")
            .ExcludeFromDescription();

        debugGroup.MapGet("/clear-cache", (HttpContext context) =>
        {
            context.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            context.Response.Headers.Add("Pragma", "no-cache");
            context.Response.Headers.Add("Expires", "0");

            return Results.Ok(new
            {
                Message = "Cache headers set to prevent caching",
                Timestamp = DateTime.UtcNow,
                Instructions = new[]
                {
                    "1. Clear browser cache (Ctrl+Shift+Delete)",
                    "2. Open browser in incognito/private mode",
                    "3. Force reload Swagger page (Ctrl+F5)",
                    "4. Check console for JavaScript errors"
                }
            });
        })
        .WithName("ClearCache");

        debugGroup.MapGet("/test-order-specific", async (IHttpClientFactory httpClientFactory) =>
        {
            var httpClient = httpClientFactory.CreateClient("DefaultClient");

            try
            {
                var orderDirectUrl = "https://localhost:60102/swagger/v1/swagger.json";
                var orderDirectResponse = await httpClient.GetAsync(orderDirectUrl);
                var orderDirectContent = await orderDirectResponse.Content.ReadAsStringAsync();

                var orderGatewayUrl = "https://localhost:7158/swagger/external/order/swagger.json";
                var orderGatewayResponse = await httpClient.GetAsync(orderGatewayUrl);
                var orderGatewayContent = await orderGatewayResponse.Content.ReadAsStringAsync();

                return Results.Ok(new
                {
                    OrderService = new
                    {
                        DirectCall = new
                        {
                            Url = orderDirectUrl,
                            StatusCode = (int)orderDirectResponse.StatusCode,
                            Success = orderDirectResponse.IsSuccessStatusCode,
                            ContentLength = orderDirectContent?.Length ?? 0,
                            HasContent = !string.IsNullOrEmpty(orderDirectContent),
                            ContentPreview = orderDirectContent?.Substring(0, Math.Min(200, orderDirectContent?.Length ?? 0))
                        },
                        GatewayProxied = new
                        {
                            Url = orderGatewayUrl,
                            StatusCode = (int)orderGatewayResponse.StatusCode,
                            Success = orderGatewayResponse.IsSuccessStatusCode,
                            ContentLength = orderGatewayContent?.Length ?? 0,
                            HasContent = !string.IsNullOrEmpty(orderGatewayContent),
                            ContentPreview = orderGatewayContent?.Substring(0, Math.Min(200, orderGatewayContent?.Length ?? 0))
                        }
                    },
                    ServiceRegistry = new
                    {
                        HasOrderService = ServiceRegistry.Services.ContainsKey("order"),
                        OrderConfig = ServiceRegistry.GetService("order"),
                        AllServices = ServiceRegistry.Services.Keys.ToArray()
                    }
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error testing Order service: {ex.Message}");
            }
        })
        .WithName("TestOrderSpecific");

        debugGroup.MapGet("/swagger-definitions", () => Results.Ok(new
        {
            definitions = ServiceRegistry.Services.Select(kvp => new
            {
                name = $"{kvp.Value.Icon} {kvp.Value.DisplayName}",
                url = $"/swagger/external/{kvp.Key}/swagger.json",
                service = kvp.Key
            }).Prepend(new
            {
                name = "🌐 API Gateway",
                url = "/swagger/v1/swagger.json",
                service = "gateway"
            }),
            instructions = "در SwaggerUI، از dropdown 'Select a definition' در بالای صفحه استفاده کنید"
        }))
        .WithName("SwaggerDefinitions");

        debugGroup.MapGet("/swagger-status", async (IHttpClientFactory httpClientFactory) =>
        {
            var httpClient = httpClientFactory.CreateClient("DefaultClient");
            var results = new Dictionary<string, object>();

            try
            {
                var gatewayUrl = "https://localhost:7158/swagger/v1/swagger.json";
                var response = await httpClient.GetAsync(gatewayUrl);
                var content = await response.Content.ReadAsStringAsync();

                results["Gateway"] = new
                {
                    Url = gatewayUrl,
                    StatusCode = (int)response.StatusCode,
                    IsSuccess = response.IsSuccessStatusCode,
                    ContentLength = content?.Length ?? 0
                };
            }
            catch (Exception ex)
            {
                results["Gateway"] = new { Error = ex.Message };
            }

            foreach (var (serviceName, config) in ServiceRegistry.Services)
            {
                try
                {
                    var fullUrl = $"https://localhost:7158/swagger/external/{serviceName}/swagger.json";
                    var response = await httpClient.GetAsync(fullUrl);
                    var content = await response.Content.ReadAsStringAsync();

                    results[config.DisplayName] = new
                    {
                        Url = fullUrl,
                        StatusCode = (int)response.StatusCode,
                        IsSuccess = response.IsSuccessStatusCode,
                        ContentLength = content?.Length ?? 0
                    };
                }
                catch (Exception ex)
                {
                    results[config.DisplayName] = new { Error = ex.Message };
                }
            }

            return Results.Ok(results);
        })
        .WithName("SwaggerDebug");

        debugGroup.MapGet("/test-swagger-direct", async (IHttpClientFactory httpClientFactory) =>
        {
            var httpClient = httpClientFactory.CreateClient("DefaultClient");
            var results = new Dictionary<string, object>();

            foreach (var (serviceName, config) in ServiceRegistry.Services)
            {
                try
                {
                    var directUrl = $"{config.BaseUrl}{config.SwaggerPath}";
                    var directResponse = await httpClient.GetAsync(directUrl);
                    var directContent = await directResponse.Content.ReadAsStringAsync();

                    var gatewayUrl = $"https://localhost:7158/swagger/external/{serviceName}/swagger.json";
                    var gatewayResponse = await httpClient.GetAsync(gatewayUrl);
                    var gatewayContent = await gatewayResponse.Content.ReadAsStringAsync();

                    results[serviceName] = new
                    {
                        DirectCall = new
                        {
                            Url = directUrl,
                            StatusCode = (int)directResponse.StatusCode,
                            Success = directResponse.IsSuccessStatusCode,
                            ContentLength = directContent?.Length ?? 0,
                            HasInternalPaths = directContent?.Contains("/api/internal/") ?? false
                        },
                        GatewayCall = new
                        {
                            Url = gatewayUrl,
                            StatusCode = (int)gatewayResponse.StatusCode,
                            Success = gatewayResponse.IsSuccessStatusCode,
                            ContentLength = gatewayContent?.Length ?? 0,
                            HasInternalPaths = gatewayContent?.Contains("/api/internal/") ?? false,
                            IsFiltered = (directContent?.Length ?? 0) != (gatewayContent?.Length ?? 0)
                        }
                    };
                }
                catch (Exception ex)
                {
                    results[serviceName] = new { Error = ex.Message };
                }
            }

            return Results.Ok(results);
        })
        .WithName("TestSwaggerDirect");

        debugGroup.MapGet("/swagger-ui-config", () =>
        {
            var swaggerUrls = ServiceRegistry.Services.Select(kvp => new
            {
                url = $"/swagger/external/{kvp.Key}/swagger.json",
                name = $"{kvp.Value.Icon} {kvp.Value.DisplayName}"
            }).Prepend(new
            {
                url = "/swagger/v1/swagger.json",
                name = "🌐 API Gateway"
            }).ToArray();

            return Results.Ok(new
            {
                SwaggerUrls = swaggerUrls,
                ExpectedInDropdown = swaggerUrls.Length,
                Services = ServiceRegistry.Services.Keys,
                ConfigurationCheck = new
                {
                    HasUserService = ServiceRegistry.Services.ContainsKey("user"),
                    HasWalletService = ServiceRegistry.Services.ContainsKey("wallet"),
                    HasOrderService = ServiceRegistry.Services.ContainsKey("order")
                }
            });
        })
        .WithName("SwaggerUIConfig");
    }
}