using BuildingBlocks.Contracts.Services;
using Microsoft.Extensions.Logging;
using Order.Infrastructure.ExternalServices.Common;

namespace Order.Infrastructure.ExternalServices.WalletPayment;

public class WalletPaymentServiceClient(HttpClient httpClient, ILogger<WalletPaymentServiceClient> logger)
    : BaseHttpClient(httpClient, logger), IWalletPaymentService
{
    public async Task<bool> CreateWalletAsync(Guid userId, CancellationToken cancellationToken)
    {
        var response = await PostAsync<CreateWalletRequest, CreateWalletResponse>(
            "/api/internal/wallet",
            new CreateWalletRequest(userId),
            cancellationToken);

        return response?.Success ?? false;
    }

    private record CreateWalletRequest(Guid UserId);
    private record CreateWalletResponse(bool Success);
}