using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WalletPayment.Domain.Common.Contracts;
using WalletPayment.Domain.TransactionAggregate;

namespace WalletPayment.Infrastructure.BackgroundServices;

/// <summary>
/// Background service for creating daily account balance snapshots
/// </summary>
public class AccountSnapshotBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AccountSnapshotBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24); // Run daily

    public AccountSnapshotBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<AccountSnapshotBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Account Snapshot Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CreateDailySnapshotsAsync(stoppingToken);
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Account Snapshot Background Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Account Snapshot Background Service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes before retry
            }
        }
    }

    private async Task CreateDailySnapshotsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating daily account snapshots");

        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            // Get all active wallets
            var wallets = await unitOfWork.Wallets.GetAllAsync(track: false, cancellationToken);

            foreach (var wallet in wallets.Where(w => w.IsActive && !w.IsDeleted))
            {
                foreach (var account in wallet.CurrencyAccounts.Where(a => a.IsActive && !a.IsDeleted))
                {
                    // Create daily snapshot
                    var snapshot = TransactionSnapshot.CreateDailySnapshot(account.Id, account.Balance);
                    await unitOfWork.Transactions.AddSnapshotAsync(snapshot, cancellationToken);
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Daily account snapshots created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create daily account snapshots");
        }
    }
}