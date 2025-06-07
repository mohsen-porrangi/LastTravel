using BuildingBlocks.Enums;

namespace BuildingBlocks.Contracts.Services
{
    /// <summary>
    /// رابط سرویس پرداخت کیف پول برای استفاده توسط سایر سرویس‌ها
    /// </summary>
    public interface IWalletPaymentService
    {
        /// <summary>
        /// ایجاد کیف پول جدید برای کاربر
        /// </summary>
        Task<bool> CreateWalletAsync(Guid userId, CancellationToken cancellationToken);

        /// <summary>
        /// بررسی موجودی کیف پول کاربر
        /// </summary>
       // Task<decimal> GetBalanceAsync(Guid userId);

        /// <summary>
        /// افزایش موجودی کیف پول
        /// </summary>
      //  Task<bool> DepositAsync(Guid userId, decimal amount, string description);

        /// <summary>
        /// برداشت از کیف پول
        /// </summary>
      //  Task<bool> WithdrawAsync(Guid userId, decimal amount, string description);

        /// <summary>
        /// انتقال مبلغ بین کیف پول‌ها
        /// </summary>
      //  Task<bool> TransferAsync(Guid fromUserId, Guid toUserId, decimal amount, string description);

        /// <summary>
        /// دریافت تاریخچه تراکنش‌های کیف پول
        /// </summary>
      //  Task<IEnumerable<WalletTransactionDto>> GetTransactionHistoryAsync(Guid userId, int pageNumber, int pageSize);
    }

    /// <summary>
    /// مدل داده تراکنش کیف پول
    /// </summary>
    public record WalletTransactionDto(
        Guid Id,
        Guid UserId,
        TransactionType Type,
        decimal Amount,
        decimal BalanceAfter,
        string Description,
        DateTime CreatedAt
    );

}