using BuildingBlocks.Contracts;
using BuildingBlocks.Domain;
using BuildingBlocks.Exceptions;
using WalletPayment.Domain.Common;
using WalletPayment.Domain.Enums;
using WalletPayment.Domain.Events;
using WalletPayment.Domain.Exceptions;
using WalletPayment.Domain.ValueObjects;

namespace WalletPayment.Domain.Aggregates.WalletAggregate;

/// <summary>
/// Wallet Aggregate Root - Main wallet entity managing all user's financial accounts
/// </summary>
public class Wallet : EntityWithDomainEvents<Guid>, IAggregateRoot, ISoftDelete
{
    private readonly List<CurrencyAccount> _currencyAccounts = new();
    private readonly List<BankAccount> _bankAccounts = new();
    private readonly List<Credit> _credits = new();

    public Guid UserId { get; private set; }
    public bool IsActive { get; private set; }

    // Collections - Expose as readonly
    public virtual IReadOnlyCollection<CurrencyAccount> CurrencyAccounts => _currencyAccounts.AsReadOnly();
    public virtual IReadOnlyCollection<BankAccount> BankAccounts => _bankAccounts.AsReadOnly();
    public virtual IReadOnlyCollection<Credit> Credits => _credits.AsReadOnly();

    // Private constructor for EF Core
    private Wallet() { }

    /// <summary>
    /// Create new wallet for user
    /// </summary>
    public Wallet(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));

        Id = Guid.NewGuid();
        UserId = userId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Emit wallet creation event
        AddDomainEvent(new WalletCreatedEvent(Id, userId));
    }

    /// <summary>
    /// Create or get currency account
    /// </summary>
    public CurrencyAccount CreateCurrencyAccount(CurrencyCode currency)
    {
        EnsureActive();

        // Check if account already exists
        var existingAccount = GetCurrencyAccount(currency);
        if (existingAccount != null && !existingAccount.IsDeleted)
        {
            return existingAccount;
        }

        // Validate business rules
        if (!BusinessRules.Wallet.CanAddCurrencyAccount(_currencyAccounts.Count(a => !a.IsDeleted)))
        {
            throw new InvalidOperationException($"Cannot exceed maximum of {BusinessRules.Wallet.MaxCurrencyAccountsPerWallet} currency accounts");
        }

        if (!BusinessRules.Currency.IsSupportedCurrency(currency))
        {
            throw new InvalidCurrencyException(currency.ToString());
        }

        // Create new account
        var account = new CurrencyAccount(Id, currency);
        _currencyAccounts.Add(account);
        UpdatedAt = DateTime.UtcNow;

        // Emit domain event
        AddDomainEvent(new CurrencyAccountCreatedEvent(Id, account.Id, currency));

        return account;
    }

    /// <summary>
    /// Add bank account
    /// </summary>
    public BankAccount AddBankAccount(
        string bankName,
        string accountNumber,
        string? cardNumber = null,
        string? shabaNumber = null,
        string? accountHolderName = null)
    {
        EnsureActive();

        // Validate business rules
        if (!BusinessRules.Wallet.CanAddBankAccount(_bankAccounts.Count(b => !b.IsDeleted)))
        {
            throw new InvalidOperationException($"Cannot exceed maximum of {BusinessRules.Wallet.MaxBankAccountsPerWallet} bank accounts");
        }

        // Check for duplicate account number
        if (_bankAccounts.Any(b => b.AccountNumber == accountNumber && !b.IsDeleted))
        {
            throw new InvalidBankAccountException($"Bank account with number {accountNumber} already exists");
        }

        var bankAccount = new BankAccount(Id, bankName, accountNumber, cardNumber, shabaNumber, accountHolderName);
        _bankAccounts.Add(bankAccount);
        UpdatedAt = DateTime.UtcNow;

        // Set as default if it's the first bank account
        if (_bankAccounts.Count(b => !b.IsDeleted) == 1)
        {
            bankAccount.SetAsDefault();
        }

        // Emit domain event
        AddDomainEvent(new BankAccountAddedEvent(Id, bankAccount.Id, bankName, accountNumber));

        return bankAccount;
    }

    /// <summary>
    /// Remove bank account (soft delete)
    /// </summary>
    public void RemoveBankAccount(Guid bankAccountId)
    {
        var bankAccount = _bankAccounts.FirstOrDefault(b => b.Id == bankAccountId && !b.IsDeleted);
        if (bankAccount == null)
        {
            throw new NotFoundException("Bank account not found", bankAccountId);
        }

        var wasDefault = bankAccount.IsDefault;
        bankAccount.SoftDelete();
        UpdatedAt = DateTime.UtcNow;

        // If removed account was default, set another as default
        if (wasDefault)
        {
            var nextDefault = _bankAccounts.FirstOrDefault(b => !b.IsDeleted && b.IsActive);
            nextDefault?.SetAsDefault();
        }
    }

    /// <summary>
    /// Get currency account by currency code
    /// </summary>
    public CurrencyAccount? GetCurrencyAccount(CurrencyCode currency)
    {
        return _currencyAccounts.FirstOrDefault(a => a.Currency == currency && !a.IsDeleted);
    }

    /// <summary>
    /// Get default currency account (IRR)
    /// </summary>
    public CurrencyAccount? GetDefaultAccount()
    {
        return GetCurrencyAccount(CurrencyCode.IRR);
    }

    /// <summary>
    /// Get active credit (B2B)
    /// </summary>
    public Credit? GetActiveCredit()
    {
        return _credits.FirstOrDefault(c => c.Status == CreditStatus.Active && !c.IsDeleted);
    }

    /// <summary>
    /// Check if wallet has sufficient balance in any currency
    /// </summary>
    public bool HasSufficientBalance(decimal amount, CurrencyCode currency)
    {
        var account = GetCurrencyAccount(currency);
        return account?.HasSufficientBalance(amount) ?? false;
    }

    /// <summary>
    /// Get total balance in IRR (converted from all currencies)
    /// </summary>
    public decimal GetTotalBalanceInIrr()
    {
        // For now, only IRR is supported
        // TODO: Add currency conversion when exchange service is ready
        return _currencyAccounts
            .Where(a => a.IsActive && !a.IsDeleted)
            .Where(a => a.Currency == CurrencyCode.IRR)
            .Sum(a => a.Balance.Value);
    }

    /// <summary>
    /// Activate wallet
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate wallet
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // Deactivate all accounts
        foreach (var account in _currencyAccounts.Where(a => !a.IsDeleted))
        {
            account.Deactivate();
        }

        foreach (var bankAccount in _bankAccounts.Where(b => !b.IsDeleted))
        {
            bankAccount.Deactivate();
        }
    }

    /// <summary>
    /// Soft delete wallet
    /// </summary>
    public void SoftDelete()
    {
        // Check if any account has positive balance
        var hasBalance = _currencyAccounts.Any(a => !a.IsDeleted && a.Balance.Value > 0);
        if (hasBalance)
        {
            throw new InvalidOperationException("Cannot delete wallet with positive balance");
        }

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // Soft delete all related entities
        foreach (var account in _currencyAccounts.Where(a => !a.IsDeleted))
        {
            account.SoftDelete();
        }

        foreach (var bankAccount in _bankAccounts.Where(b => !b.IsDeleted))
        {
            bankAccount.SoftDelete();
        }
    }

    #region B2B Credit Methods (Future Implementation)

    /// <summary>
    /// Assign credit limit (B2B)
    /// </summary>
    public void AssignCredit(decimal amount, DateTime dueDate, string description)
    {
        EnsureActive();

        if (GetActiveCredit() != null)
        {
            throw new InvalidOperationException("Active credit already exists");
        }

        var creditMoney = Money.Create(amount, CurrencyCode.IRR);
        if (!BusinessRules.Credit.IsValidCreditAmount(creditMoney))
        {
            throw new ArgumentException("Invalid credit amount");
        }

        if (!BusinessRules.Credit.IsValidCreditDueDate(dueDate))
        {
            throw new ArgumentException("Invalid credit due date");
        }

        var credit = new Credit(Id, creditMoney, dueDate, description);
        _credits.Add(credit);
        UpdatedAt = DateTime.UtcNow;

        // Emit domain event
        AddDomainEvent(new CreditAssignedEvent(Id, UserId, amount, dueDate));
    }

    #endregion

    #region Private Methods

    private void EnsureActive()
    {
        if (!IsActive)
            throw new InvalidOperationException("Wallet is not active");

        if (IsDeleted)
            throw new InvalidOperationException("Wallet has been deleted");
    }

    #endregion
}