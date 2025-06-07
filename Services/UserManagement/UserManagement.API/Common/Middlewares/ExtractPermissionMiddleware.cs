namespace UserManagement.API.Common.Middlewares
{
    public class ExtractPermissionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExtractPermissionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var permissionAttr = endpoint?.Metadata.GetMetadata<RequirePermissionAttribute>();

            if (permissionAttr != null)
            {
                context.Items["RequiredPermission"] = permissionAttr.Permission;
            }

            await _next(context);
        }
    }
}
