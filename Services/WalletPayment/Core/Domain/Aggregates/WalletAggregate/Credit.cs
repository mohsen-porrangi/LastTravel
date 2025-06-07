using BuildingBlocks.Domain;
using WalletPayment.Domain.Enums;
using WalletPayment.Domain.Events;
using WalletPayment.Domain.Exceptions;
using WalletPayment.Domain.ValueObjects;

namespace WalletPayment.Domain.Aggregates.WalletAggregate;

/// <summary>
/// Credit entity for B2B customers - اعتبار برای مشتریان تجاری
/// </summary>
public class Credit : BaseEntity<Guid>
{
    public Guid WalletId { get; private set; }
    public Money CreditLimit { get; private set; } = null!;
    public Money UsedCredit { get; private set; } = null!;

    /// <summary>
    /// اعتبار باقی‌مانده
    /// </summary>
    public Money AvailableCredit => Money.Create(
        CreditLimit.Value - UsedCredit.Value,
        CreditLimit.Currency);

    public DateTime GrantedDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? SettledDate { get; private set; }
    public CreditStatus Status { get; private set; }
    public string Description { get; private set; } = string.Empty;

    // Settlement transaction reference
    public Guid? SettlementTransactionId { get; private set; }

    // Navigation property
    public virtual Wallet Wallet { get; private set; } = null!;

    // Private constructor for EF Core
    private Credit() { }

    /// <summary>
    /// Create new credit assignment
    /// </summary>
    public Credit(
        Guid walletId,
        Money creditLimit,
        DateTime dueDate,
        string description)
    {
        if (walletId == Guid.Empty)
            throw new ArgumentException("WalletId cannot be empty", nameof(walletId));

        if (dueDate <= DateTime.UtcNow)
            throw new ArgumentException("Due date must be in the future", nameof(dueDate));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        if (creditLimit == null)
            throw new ArgumentNullException(nameof(creditLimit));

        if (creditLimit.Value <= 0)
            throw new ArgumentException("Credit limit must be positive", nameof(creditLimit));

        Id = Guid.NewGuid();
        WalletId = walletId;
        CreditLimit = creditLimit;
        UsedCredit = Money.Zero(creditLimit.Currency);
        GrantedDate = DateTime.UtcNow;
        DueDate = dueDate;
        Status = CreditStatus.Active;
        Description = description.Trim();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Use part of the credit - استفاده از بخشی از اعتبار
    /// </summary>
    public Money UseCredit(Money amount)
    {
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));

        if (amount.Currency != CreditLimit.Currency)
            throw new ArgumentException("Currency mismatch", nameof(amount));

        if (Status != CreditStatus.Active)
            throw new InvalidOperationException($"Cannot use credit with status {Status}");

        if (DateTime.UtcNow > DueDate)
        {
            Status = CreditStatus.Overdue;
            UpdatedAt = DateTime.UtcNow;
            throw new InvalidOperationException("Credit is overdue");
        }

        var newUsedCredit = Money.Create(
            UsedCredit.Value + amount.Value,
            UsedCredit.Currency);

        if (newUsedCredit.Value > CreditLimit.Value)
        {
            var availableAmount = Money.Create(
                CreditLimit.Value - UsedCredit.Value,
                CreditLimit.Currency);
            throw new InsufficientBalanceException(
                WalletId, amount.Value, availableAmount.Value);
        }

        UsedCredit = newUsedCredit;
        UpdatedAt = DateTime.UtcNow;

        return AvailableCredit;
    }

    /// <summary>
    /// Check if credit can be used for amount - بررسی امکان استفاده از اعتبار
    /// </summary>
    public bool CanUseCredit(Money amount)
    {
        if (amount == null || Status != CreditStatus.Active)
            return false;

        if (DateTime.UtcNow > DueDate)
            return false;

        if (amount.Currency != CreditLimit.Currency)
            return false;

        return AvailableCredit.Value >= amount.Value;
    }

    /// <summary>
    /// Settle the credit - تسویه اعتبار
    /// </summary>
    public void Settle(Guid settlementTransactionId)
    {
        if (settlementTransactionId == Guid.Empty)
            throw new ArgumentException("Settlement transaction ID cannot be empty", nameof(settlementTransactionId));

        if (Status == CreditStatus.Settled)
            throw new InvalidOperationException("Credit is already settled");

        Status = CreditStatus.Settled;
        SettledDate = DateTime.UtcNow;
        SettlementTransactionId = settlementTransactionId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark credit as overdue - علامت‌گذاری به عنوان سررسید گذشته
    /// </summary>
    public void MarkAsOverdue()
    {
        if (Status != CreditStatus.Active)
            throw new InvalidOperationException("Only active credits can be marked as overdue");

        Status = CreditStatus.Overdue;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if credit is overdue - بررسی سررسید گذشته بودن
    /// </summary>
    public bool IsOverdue()
    {
        return DateTime.UtcNow > DueDate && Status == CreditStatus.Active;
    }

    /// <summary>
    /// Extend credit due date - تمدید تاریخ سررسید
    /// </summary>
    public void ExtendDueDate(DateTime newDueDate)
    {
        if (newDueDate <= DateTime.UtcNow)
            throw new ArgumentException("New due date must be in the future", nameof(newDueDate));

        if (newDueDate <= DueDate)
            throw new ArgumentException("New due date must be later than current due date", nameof(newDueDate));

        DueDate = newDueDate;
        UpdatedAt = DateTime.UtcNow;

        // Reactivate if it was overdue
        if (Status == CreditStatus.Overdue)
        {
            Status = CreditStatus.Active;
        }
    }

    /// <summary>
    /// Get credit summary - دریافت خلاصه اعتبار
    /// </summary>
    public string GetSummary()
    {
        return $"Credit: {AvailableCredit.Value:N0} / {CreditLimit.Value:N0} {CreditLimit.Currency} - {Status} - Due: {DueDate:yyyy-MM-dd}";
    }
}
