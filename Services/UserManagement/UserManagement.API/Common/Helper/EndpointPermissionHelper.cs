namespace UserManagement.API.Common.Helper
{
    public static class EndpointPermissionHelper
    {
        public static string? GetRequiredPermission(HttpContext context)
        {
            return context.GetEndpoint()?.Metadata.GetMetadata<RequirePermissionAttribute>()?.Permission;
        }
    }
}
