namespace YarpApiGateway.Middleware;

public class InternalPathFilterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InternalPathFilterMiddleware> _logger;
    private readonly HashSet<string> _internalPaths;

    public InternalPathFilterMiddleware(
        RequestDelegate next,
        ILogger<InternalPathFilterMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _internalPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/api/internal/",
            "/internal/",
            "/admin/internal/"
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();

        if (IsInternalPath(path))
        {
            _logger.LogWarning("Blocked internal API access attempt: {Path} from {RemoteIP}",
                path, context.Connection.RemoteIpAddress);

            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "Forbidden",
                message = "Access to internal APIs is not allowed via gateway",
                statusCode = 403,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path.Value
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorResponse));
            return;
        }        
        if (context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            _logger.LogDebug("Gateway Request: {Method} {Path} -> Forwarding to backend",
                context.Request.Method, path);
        }

        await _next(context);
    }

    private bool IsInternalPath(string? path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        return _internalPaths.Any(internalPath =>
            path.Contains(internalPath, StringComparison.OrdinalIgnoreCase));
    }
}