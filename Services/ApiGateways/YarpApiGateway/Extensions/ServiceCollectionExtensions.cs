using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using YarpApiGateway.Configuration;
using YarpApiGateway.Endpoints;
using YarpApiGateway.Services;

namespace YarpApiGateway.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerConfiguration();
        services.AddHttpClientConfiguration();
        services.AddYarpConfiguration(configuration);
        services.AddRateLimitingConfiguration();
        services.AddCorsConfiguration();
        services.AddGatewayCustomServices();

        return services;
    }

    private static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "آفاق سیر - API Gateway",
                Version = "v1",
                Description = "API Gateway برای دسترسی به تمام سرویس‌های آفاق سیر"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    private static IServiceCollection AddHttpClientConfiguration(this IServiceCollection services)
    {
        services.AddHttpClient("DefaultClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                // در development محیط، SSL validation را bypass می‌کنیم
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    return true;

                // در production، certificate validation انجام شود
                return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
            }
        });

        services.AddHttpClient();
        return services;
    }

    private static IServiceCollection AddYarpConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));

        return services;
    }

    private static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services)
    {
        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
            {
                options.Window = TimeSpan.FromSeconds(10);
                options.PermitLimit = 100;
                options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 10;
            });

            rateLimiterOptions.AddFixedWindowLimiter("swagger", options =>
            {
                options.Window = TimeSpan.FromSeconds(60);
                options.PermitLimit = 20; // محدودیت کمتر برای swagger
                options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 5;
            });
        });

        return services;
    }

    private static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });

            options.AddPolicy("SwaggerCors", policy =>
            {
                var serviceUrls = ServiceRegistry.Services.Values.Select(s => s.BaseUrl).ToArray();
                policy.WithOrigins(serviceUrls.Concat(new[] { "https://localhost:7158" }).ToArray())
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }

    private static IServiceCollection AddGatewayCustomServices(this IServiceCollection services)
    {
        services.AddScoped<ISwaggerService, SwaggerService>();

        return services;
    }
}

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureGatewayPipeline(this WebApplication app)
    {
        // Development-specific configuration
        if (app.Environment.IsDevelopment())
        {
            app.ConfigureSwaggerUI();
        }

        // Configure middleware pipeline
        app.UseCors("AllowAll");
        app.UseRateLimiter();
        app.UseStaticFiles();

        // جلوگیری از Cache در Development
        app.UseNoCacheInDevelopment();

        // Custom middlewares (بهترتیب اولویت)
        app.UseMiddleware<YarpApiGateway.Middleware.RequestLoggingMiddleware>();
        app.UseMiddleware<YarpApiGateway.Middleware.SwaggerRouteMiddleware>();
        app.UseMiddleware<YarpApiGateway.Middleware.InternalPathFilterMiddleware>();

        // Map endpoints
        app.MapGatewayEndpoints();

        // YARP reverse proxy - MUST BE LAST
        app.MapReverseProxy();

        return app;
    }

    private static void ConfigureSwaggerUI(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            // Gateway Swagger
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "🌐 API Gateway");

            // External services Swagger
            foreach (var (serviceName, config) in ServiceRegistry.Services.OrderBy(x => x.Value.DisplayName))
            {
                var endpointUrl = $"/swagger/external/{serviceName}/swagger.json";
                var displayName = $"{config.Icon} {config.DisplayName}";

                c.SwaggerEndpoint(endpointUrl, displayName);
            }

            c.RoutePrefix = "swagger";
            c.DocumentTitle = "آفاق سیر - API Documentation";
            c.DefaultModelsExpandDepth(2);
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            c.EnableDeepLinking();
            c.DisplayRequestDuration();
            c.EnableTryItOutByDefault();
            c.ShowExtensions();
            c.EnableFilter();
            c.ShowCommonExtensions();
            c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
            c.MaxDisplayedTags(50);

            // جلوگیری از Cache در Development
            if (app.Environment.IsDevelopment())
            {
                // اضافه کردن timestamp به URLs برای جلوگیری از cache
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                // Override کردن URLs با timestamp
                c.ConfigObject.AdditionalItems["urls"] = ServiceRegistry.Services.Select(kvp => new
                {
                    url = $"/swagger/external/{kvp.Key}/swagger.json?v={timestamp}",
                    name = $"{kvp.Value.Icon} {kvp.Value.DisplayName}"
                }).Prepend(new
                {
                    url = $"/swagger/v1/swagger.json?v={timestamp}",
                    name = "🌐 API Gateway"
                }).ToArray();
            }

            // Custom JavaScript if exists
            if (File.Exists(Path.Combine(app.Environment.WebRootPath, "swagger-ui", "custom.js")))
            {
                c.InjectJavascript("/swagger-ui/custom.js");
            }

            // Custom CSS if exists
            if (File.Exists(Path.Combine(app.Environment.WebRootPath, "swagger-ui", "custom.css")))
            {
                c.InjectStylesheet("/swagger-ui/custom.css");
            }
        });
    }

    public static IApplicationBuilder UseNoCacheInDevelopment(this IApplicationBuilder app)
    {
        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

        if (env.IsDevelopment())
        {
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value?.ToLower();

                // برای swagger و static files، cache headers اضافه کن
                if (path?.Contains("/swagger") == true ||
                    path?.Contains("/api/") == true)
                {
                    context.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                    context.Response.Headers.Add("Pragma", "no-cache");
                    context.Response.Headers.Add("Expires", "0");
                }

                await next();
            });
        }

        return app;
    }
}