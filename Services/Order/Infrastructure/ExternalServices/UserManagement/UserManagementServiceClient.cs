using BuildingBlocks.Contracts.Services;
using Microsoft.Extensions.Logging;
using Order.Infrastructure.ExternalServices.Common;

namespace Order.Infrastructure.ExternalServices.UserManagement;

public class UserManagementServiceClient(HttpClient httpClient, ILogger<UserManagementServiceClient> logger)
    : BaseHttpClient(httpClient, logger), IUserManagementService
{
    public async Task<bool> UserExistsAsync(Guid userId)
    {
        var response = await GetAsync<UserExistsResponse>($"/api/internal/users/{userId}/exists");
        return response?.Exists ?? false;
    }

    public async Task<bool> IsUserActiveAsync(Guid userId)
    {
        var response = await GetAsync<UserStatusResponse>($"/api/internal/users/{userId}/status");
        return response?.IsActive ?? false;
    }

    public Task<bool> ValidateCredentialsAsync(string mobile, string password)
    {
        throw new NotImplementedException("Not needed for Order service");
    }

    public Task<TokenResponseDto> AuthenticateAsync(AuthRequestDto request)
    {
        throw new NotImplementedException("Not needed for Order service");
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        throw new NotImplementedException("Not needed for Order service");
    }

    //public Task<bool> HasPermissionAsync(Guid userId, string permissionCode)
    //{
    //    throw new NotImplementedException("Not needed for Order service");
    //}

    public async Task<bool> HasPermissionAsync(Guid userId, string permissionCode)
    {
        var response = await PostAsync<CheckPermissionRequest, CheckPermissionResponse>(
            "/api/internal/permissions/check",
            new CheckPermissionRequest(userId, permissionCode));

        return response?.IsGranted ?? false;
    }

    public async Task<UserDetailDto> GetUserByIdAsync(Guid userId)
    {
        var response = await GetAsync<UserDetailDto>($"/api/internal/users/{userId}");
        return response ?? throw new Exception($"User {userId} not found");
    }

    private record UserExistsResponse(bool Exists);
    private record UserStatusResponse(bool IsActive);
    private record CheckPermissionRequest(Guid UserId, string PermissionCode);
    private record CheckPermissionResponse(bool IsGranted);
}