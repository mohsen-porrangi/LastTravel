using BuildingBlocks.Contracts.Services;

namespace UserManagement.API.Services;
public class WalletPaymentService : IWalletPaymentService
{
    private readonly HttpClient _httpClient;

    public WalletPaymentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> CreateWalletAsync(Guid userId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/internal/wallet", new { userId }, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
