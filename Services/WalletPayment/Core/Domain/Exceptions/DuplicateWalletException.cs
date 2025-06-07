using BuildingBlocks.Exceptions;

namespace WalletPayment.Domain.Exceptions;
/// <summary>
/// خطای کیف پول تکراری
/// </summary>
public class DuplicateWalletException : ConflictDomainException
{
    public DuplicateWalletException(Guid userId)
        : base("کیف پول تکراری", $"کاربر {userId} قبلاً دارای کیف پول است")
    {
        UserId = userId;
    }

    public Guid UserId { get; }
}

/// <summary>
/// خطای حساب بانکی نامعتبر
/// </summary>
public class InvalidBankAccountException : BadRequestException
{
    public InvalidBankAccountException(string reason)
        : base("حساب بانکی نامعتبر", reason)
    {
    }
}
