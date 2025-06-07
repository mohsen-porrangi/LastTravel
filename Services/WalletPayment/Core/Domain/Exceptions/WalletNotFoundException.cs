using BuildingBlocks.Exceptions;

namespace WalletPayment.Domain.Exceptions;

/// <summary>
/// خطای کیف پول یافت نشد
/// </summary>
public class WalletNotFoundException : NotFoundException
{
    public WalletNotFoundException(Guid userId)
        : base("کیف پول یافت نشد", $"کاربر: {userId}")
    {
        UserId = userId;
    }

    public WalletNotFoundException(Guid walletId, string context)
        : base("کیف پول یافت نشد", $"شناسه: {walletId}, زمینه: {context}")
    {
        WalletId = walletId;
    }

    public Guid? UserId { get; }
    public Guid? WalletId { get; }
}