using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.Enums;

namespace WalletPayment.Infrastructure.BackgroundServices;

/// <summary>
/// Background service for checking credit due dates (B2B)
/// </summary>
public class CreditDueDateCheckingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CreditDueDateCheckingService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(6); // Run every 6 hours

    public CreditDueDateCheckingService(
        IServiceProvider serviceProvider,
        ILogger<CreditDueDateCheckingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Credit Due Date Checking Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckCreditDueDatesAsync(stoppingToken);
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Credit Due Date Checking Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Credit Due Date Checking Service");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken); // Wait 10 minutes before retry
            }
        }
    }

    private async Task CheckCreditDueDatesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking credit due dates");

        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            // Check for credits due soon (7 days warning)
            var walletsDueSoon = await unitOfWork.Wallets.GetWalletsWithCreditsDueSoonAsync(7, cancellationToken);

            foreach (var wallet in walletsDueSoon)
            {
                var activeCredit = wallet.GetActiveCredit();
                if (activeCredit != null)
                {
                    _logger.LogWarning("Credit due soon for wallet {WalletId}, user {UserId}, due date: {DueDate}",
                        wallet.Id, wallet.UserId, activeCredit.DueDate);

                    // TODO: Send notification to user
                }
            }

            // Check for overdue credits
            var walletsOverdue = await unitOfWork.Wallets.GetWalletsWithOverdueCreditsAsync(cancellationToken);

            foreach (var wallet in walletsOverdue)
            {
                var activeCredit = wallet.GetActiveCredit();
                if (activeCredit != null)
                {
                    _logger.LogWarning("Credit overdue for wallet {WalletId}, user {UserId}, due date: {DueDate}",
                        wallet.Id, wallet.UserId, activeCredit.DueDate);

                    // Mark credit as overdue
                    activeCredit.MarkAsOverdue();

                    // TODO: Send urgent notification and possibly suspend account
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Credit due date check completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check credit due dates");
        }
    }
}