namespace YarpApiGateway.Middleware;
public class SwaggerRouteMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SwaggerRouteMiddleware> _logger;
    private readonly HashSet<string> _gatewayPaths;
    private readonly HashSet<string> _staticExtensions;

    public SwaggerRouteMiddleware(
        RequestDelegate next,
        ILogger<SwaggerRouteMiddleware> logger)
    {
        _next = next;
        _logger = logger;

        _gatewayPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/swagger",
            "/swagger/",
            "/swagger/v1/swagger.json",
            "/swagger/index.html",
            "/swagger/external/",
            "/health",
            "/gateway/",
            "/test/",
            "/debug/",
            "/monitor/",
            "/swagger-ui"
        };

        _staticExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".css", ".js", ".ico", ".png", ".html", ".json", ".woff", ".woff2", ".ttf", ".svg"
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();

        // اگر درخواست مربوط به gateway است
        if (IsGatewayPath(path))
        {
            _logger.LogDebug("Gateway request: {Path}", path);
            await _next(context);
            return;
        }

        // اگر درخواست مربوط به static files است
        if (IsStaticFile(path))
        {
            await _next(context);
            return;
        }

        // بقیه درخواست‌ها به YARP ارسال می‌شوند
        _logger.LogDebug("Proxying request: {Path}", path);
        await _next(context);
    }

    private bool IsGatewayPath(string? path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        return _gatewayPaths.Any(gatewayPath =>
            path.StartsWith(gatewayPath, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsStaticFile(string? path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        return path.Contains("/swagger-ui/", StringComparison.OrdinalIgnoreCase) ||
               _staticExtensions.Any(ext => path.Contains(ext, StringComparison.OrdinalIgnoreCase));
    }
}