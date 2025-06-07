using BuildingBlocks.Exceptions;

namespace WalletPayment.Domain.Exceptions;

/// <summary>
/// خطای موجودی ناکافی
/// </summary>
public class InsufficientBalanceException : BadRequestException
{
    public InsufficientBalanceException(Guid walletId, decimal requested, decimal available)
        : base(
            "موجودی کیف پول کافی نیست",
            $"درخواست: {requested:N0}, موجودی: {available:N0}, کیف پول: {walletId}")
    {
        WalletId = walletId;
        RequestedAmount = requested;
        AvailableBalance = available;
    }

    public Guid WalletId { get; }
    public decimal RequestedAmount { get; }
    public decimal AvailableBalance { get; }
}