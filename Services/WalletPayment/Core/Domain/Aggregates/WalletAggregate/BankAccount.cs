using BuildingBlocks.Contracts;
using BuildingBlocks.Domain;
using WalletPayment.Domain.Exceptions;
using WalletPayment.Domain.ValueObjects;

namespace WalletPayment.Domain.Aggregates.WalletAggregate;

/// <summary>
/// Entity حساب بانکی - برای برگشت وجه و انتقال به حساب واقعی
/// </summary>
public class BankAccount : EntityWithDomainEvents<Guid>, ISoftDelete
{
    public Guid WalletId { get; private set; }
    public string BankName { get; private set; } = string.Empty;
    public string AccountNumber { get; private set; } = string.Empty;
    public string? CardNumber { get; private set; }
    public string? ShabaNumber { get; private set; }
    public string? AccountHolderName { get; private set; }
    public bool IsVerified { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation Property
    public Wallet Wallet { get; private set; } = null!;

    // Private constructor for EF Core
    private BankAccount() { }

    /// <summary>
    /// ایجاد حساب بانکی جدید
    /// </summary>
    public BankAccount(
        Guid walletId,
        string bankName,
        string accountNumber,
        string? cardNumber = null,
        string? shabaNumber = null,
        string? accountHolderName = null)
    {
        if (walletId == Guid.Empty)
            throw new ArgumentException("شناسه کیف پول نمی‌تواند خالی باشد", nameof(walletId));

        ValidateInputs(bankName, accountNumber, cardNumber, shabaNumber);

        Id = Guid.NewGuid();
        WalletId = walletId;
        BankName = bankName.Trim();
        AccountNumber = accountNumber.Trim();
        CardNumber = cardNumber?.Trim();
        ShabaNumber = shabaNumber?.Trim();
        AccountHolderName = accountHolderName?.Trim();
        IsVerified = false;
        IsDefault = false;
        IsActive = true;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// تنظیم به عنوان حساب پیش‌فرض
    /// </summary>
    public void SetAsDefault()
    {
        EnsureActive();
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// برداشتن از حالت پیش‌فرض
    /// </summary>
    public void UnsetAsDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// تایید حساب بانکی
    /// </summary>
    public void Verify()
    {
        EnsureActive();
        IsVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// لغو تایید حساب بانکی
    /// </summary>
    public void Unverify()
    {
        IsVerified = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// فعال/غیرفعال کردن حساب
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        IsDefault = false; // اگر غیرفعال شد، پیش‌فرض هم نیست
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// حذف منطقی حساب
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        IsActive = false;
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// بروزرسانی اطلاعات حساب
    /// </summary>
    public void UpdateInfo(
        string? bankName = null,
        string? accountHolderName = null)
    {
        EnsureActive();

        if (!string.IsNullOrWhiteSpace(bankName))
        {
            if (bankName.Length > 100)
                throw new InvalidBankAccountException("نام بانک نمی‌تواند بیش از 100 کاراکتر باشد");

            BankName = bankName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(accountHolderName))
        {
            if (accountHolderName.Length > 200)
                throw new InvalidBankAccountException("نام صاحب حساب نمی‌تواند بیش از 200 کاراکتر باشد");

            AccountHolderName = accountHolderName.Trim();
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// دریافت شماره حساب ماسک شده (امنیت)
    /// </summary>
    public string GetMaskedAccountNumber()
    {
        if (AccountNumber.Length <= 4)
            return AccountNumber;

        return "****" + AccountNumber[^4..];
    }

    /// <summary>
    /// دریافت شماره کارت ماسک شده (امنیت)
    /// </summary>
    public string GetMaskedCardNumber()
    {
        if (string.IsNullOrEmpty(CardNumber) || CardNumber.Length <= 4)
            return CardNumber ?? string.Empty;

        return "**** **** **** " + CardNumber[^4..];
    }

    /// <summary>
    /// اعتبارسنجی ورودی‌ها
    /// </summary>
    private static void ValidateInputs(string bankName, string accountNumber, string? cardNumber, string? shabaNumber)
    {
        // نام بانک
        if (string.IsNullOrWhiteSpace(bankName))
            throw new InvalidBankAccountException("نام بانک الزامی است");
        if (bankName.Length > 100)
            throw new InvalidBankAccountException("نام بانک نمی‌تواند بیش از 100 کاراکتر باشد");

        // شماره حساب
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new InvalidBankAccountException("شماره حساب الزامی است");
        if (!accountNumber.All(char.IsDigit))
            throw new InvalidBankAccountException("شماره حساب باید فقط شامل عدد باشد");
        if (accountNumber.Length < 10 || accountNumber.Length > 20)
            throw new InvalidBankAccountException("شماره حساب باید بین 10 تا 20 رقم باشد");

        // شماره کارت (اختیاری)
        if (!string.IsNullOrEmpty(cardNumber))
        {
            if (!cardNumber.All(char.IsDigit))
                throw new InvalidBankAccountException("شماره کارت باید فقط شامل عدد باشد");
            if (cardNumber.Length != 16)
                throw new InvalidBankAccountException("شماره کارت باید 16 رقم باشد");
            if (!IsValidCardNumber(cardNumber))
                throw new InvalidBankAccountException("شماره کارت نامعتبر است");
        }

        // شماره شبا (اختیاری)
        if (!string.IsNullOrEmpty(shabaNumber))
        {
            if (!shabaNumber.StartsWith("IR") || shabaNumber.Length != 26)
                throw new InvalidBankAccountException("شماره شبا باید با IR شروع شده و 26 کاراکتر باشد");
            if (!IsValidIban(shabaNumber))
                throw new InvalidBankAccountException("شماره شبا نامعتبر است");
        }
    }

    /// <summary>
    /// اعتبارسنجی شماره کارت (الگوریتم Luhn)
    /// </summary>
    private static bool IsValidCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length != 16 || !cardNumber.All(char.IsDigit))
            return false;

        int sum = 0;
        bool isEven = false;

        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            int digit = cardNumber[i] - '0';

            if (isEven)
            {
                digit *= 2;
                if (digit > 9)
                    digit -= 9;
            }

            sum += digit;
            isEven = !isEven;
        }

        return sum % 10 == 0;
    }

    /// <summary>
    /// اعتبارسنجی شماره شبا
    /// </summary>
    private static bool IsValidIban(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban) || iban.Length != 26 || !iban.StartsWith("IR"))
            return false;

        // اعتبارسنجی ساده - در آینده می‌تواند کاملتر شود
        return iban[2..].All(char.IsDigit);
    }

    /// <summary>
    /// اعمال قوانین کسب و کار
    /// </summary>
    private void EnsureActive()
    {
        if (!IsActive)
            throw new InvalidOperationException("حساب بانکی غیرفعال است");

        if (IsDeleted)
            throw new InvalidOperationException("حساب بانکی حذف شده است");
    }

    /// <summary>
    /// دریافت اطلاعات خلاصه حساب
    /// </summary>
    public string GetSummary()
    {
        var status = IsVerified ? "تایید شده" : "تایید نشده";
        var defaultText = IsDefault ? " (پیش‌فرض)" : "";
        return $"{BankName} - {GetMaskedAccountNumber()} - {status}{defaultText}";
    }
}