namespace YarpApiGateway.Constants;

public static class GatewayConstants
{
    public static class Endpoints
    {
        public const string Health = "/health";
        public const string Swagger = "/swagger";
        public const string SwaggerExternal = "/swagger/external";
        public const string Gateway = "/gateway";
        public const string Test = "/test";
        public const string Debug = "/debug";
    }

    public static class Policies
    {
        public const string AllowAll = "AllowAll";
        public const string SwaggerCors = "SwaggerCors";
        public const string FixedWindow = "fixed";
        public const string SwaggerRateLimit = "swagger";
    }

    public static class Tags
    {
        public const string Gateway = "Gateway";
        public const string Health = "Health";
        public const string Swagger = "Swagger";
        public const string Testing = "Testing";
        public const string Debug = "Debug";
    }

    public static class HttpClients
    {
        public const string Default = "DefaultClient";
    }

    public static class Headers
    {
        public const string RequestId = "X-Request-ID";
        public const string ForwardedFor = "X-Forwarded-For";
        public const string RealIp = "X-Real-IP";
    }

    public static class InternalPaths
    {
        public static readonly string[] Patterns =
        {
            "/api/internal/",
            "/internal/",
            "/admin/internal/"
        };
    }

    public static class StaticExtensions
    {
        public static readonly string[] Extensions =
        {
            ".css", ".js", ".ico", ".png", ".html", ".json",
            ".woff", ".woff2", ".ttf", ".svg", ".map"
        };
    }
}

public static class LoggingConstants
{
    public static class EventIds
    {
        public const int InternalApiBlocked = 1001;
        public const int SwaggerRequested = 1002;
        public const int ServiceHealthCheck = 1003;
        public const int ProxyRequest = 1004;
        public const int ErrorOccurred = 5000;
    }

    public static class Messages
    {
        public const string InternalApiBlocked = "Blocked internal API access attempt: {Path} from {RemoteIP}";
        public const string ServiceUnreachable = "Service {ServiceName} is unreachable at {Url}";
        public const string SwaggerFiltered = "Filtered internal APIs from swagger for service {ServiceName}";
    }
}