using FluentValidation;
using WalletPayment.Domain.Common;
using WalletPayment.Domain.Enums;
using WalletPayment.Domain.ValueObjects;

namespace WalletPayment.Application.Common.Validation;

/// <summary>
/// Wallet-specific validation extensions for FluentValidation
/// ✅ PRIMARY VALIDATION FILE - فایل اصلی validation
/// </summary>
public static class WalletValidationExtensions
{
    #region Transaction Validation

    /// <summary>
    /// Validate transaction amount for specific currency
    /// </summary>
    public static IRuleBuilderOptions<T, decimal> ValidateTransactionAmount<T>(
        this IRuleBuilder<T, decimal> ruleBuilder,
        CurrencyCode currency = CurrencyCode.IRR)
    {
        return ruleBuilder
            .GreaterThan(0).WithMessage("مبلغ باید بزرگتر از صفر باشد")
            .Must(amount => BusinessRules.Amounts.IsValidTransactionAmount(Money.Create(amount, currency)))
            .WithMessage($"مبلغ باید بین {BusinessRules.Amounts.MinimumTransactionAmount.Value:N0} تا {BusinessRules.Amounts.MaximumSingleTransactionAmount.Value:N0} ریال باشد");
    }

    /// <summary>
    /// Validate supported currency
    /// </summary>
    public static IRuleBuilderOptions<T, CurrencyCode> ValidateSupportedCurrency<T>(
        this IRuleBuilder<T, CurrencyCode> ruleBuilder)
    {
        return ruleBuilder
            .Must(BusinessRules.Currency.IsSupportedCurrency)
            .WithMessage("ارز انتخابی پشتیبانی نمی‌شود");
    }

    /// <summary>
    /// Validate transaction description
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidateTransactionDescription<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("توضیحات تراکنش الزامی است")
            .MaximumLength(500).WithMessage("توضیحات نباید بیش از 500 کاراکتر باشد");
    }

    #endregion

    #region Bank Account Validation

    /// <summary>
    /// Validate Iranian bank account number
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidateBankAccountNumber<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("شماره حساب الزامی است")
            .Matches(@"^\d{10,20}$").WithMessage("شماره حساب باید بین 10 تا 20 رقم باشد")
            .Must(DomainValidationRules.Iranian.IsValidAccountNumber)
            .WithMessage("شماره حساب نامعتبر است");
    }

    /// <summary>
    /// Validate Iranian card number
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidateIranianCardNumber<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Matches(@"^\d{16}$").WithMessage("شماره کارت باید 16 رقم باشد")
            .Must(DomainValidationRules.Iranian.IsValidCardNumber!)
            .WithMessage("شماره کارت نامعتبر است");
    }

    /// <summary>
    /// Validate Iranian SHABA number
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidateIranianShabaNumber<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Matches(@"^IR\d{24}$").WithMessage("شماره شبا باید با IR شروع شده و 26 کاراکتر باشد")
            .Must(DomainValidationRules.Iranian.IsValidShabaNumber!)
            .WithMessage("شماره شبا نامعتبر است");
    }

    /// <summary>
    /// Validate bank name
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidateBankName<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("نام بانک الزامی است")
            .MaximumLength(100).WithMessage("نام بانک نباید بیش از 100 کاراکتر باشد");
    }

    #endregion

    #region Credit Validation (B2B)

    /// <summary>
    /// Validate credit amount
    /// </summary>
    public static IRuleBuilderOptions<T, decimal> ValidateCreditAmount<T>(
        this IRuleBuilder<T, decimal> ruleBuilder,
        CurrencyCode currency = CurrencyCode.IRR)
    {
        return ruleBuilder
            .GreaterThan(0).WithMessage("مبلغ اعتبار باید بزرگتر از صفر باشد")
            .Must(amount => BusinessRules.Credit.IsValidCreditAmount(Money.Create(amount, currency)))
            .WithMessage($"مبلغ اعتبار نباید بیش از {BusinessRules.Credit.MaximumCreditLimit.Value:N0} ریال باشد");
    }

    /// <summary>
    /// Validate credit due date
    /// </summary>
    public static IRuleBuilderOptions<T, DateTime> ValidateCreditDueDate<T>(
        this IRuleBuilder<T, DateTime> ruleBuilder)
    {
        return ruleBuilder
            .Must(BusinessRules.Credit.IsValidCreditDueDate)
            .WithMessage($"تاریخ سررسید باید بین امروز تا {BusinessRules.Credit.MaximumCreditDurationDays} روز آینده باشد");
    }

    #endregion

    #region Payment Validation

    /// <summary>
    /// Validate callback URL
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidateCallbackUrl<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("آدرس بازگشت الزامی است")
            .Must(DomainValidationRules.Business.IsValidUrl!)
            .WithMessage("آدرس بازگشت نامعتبر است");
    }

    /// <summary>
    /// Validate payment authority
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidatePaymentAuthority<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("شناسه مرجع پرداخت الزامی است")
            .Length(36).WithMessage("شناسه مرجع پرداخت نامعتبر است");
    }

    #endregion

    #region User Validation

    /// <summary>
    /// Validate user ID
    /// </summary>
    public static IRuleBuilderOptions<T, Guid> ValidateUserId<T>(
        this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("شناسه کاربر الزامی است");
    }

    /// <summary>
    /// Validate wallet ID
    /// </summary>
    public static IRuleBuilderOptions<T, Guid> ValidateWalletId<T>(
        this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("شناسه کیف پول الزامی است");
    }

    #endregion

    #region Transfer Validation

    /// <summary>
    /// Validate transfer users are different
    /// </summary>
    public static IRuleBuilderOptions<T, Guid> ValidateTransferUsers<T>(
        this IRuleBuilder<T, Guid> ruleBuilder)
        where T : class
    {
        return ruleBuilder
            .NotEmpty().WithMessage("شناسه کاربر مقصد الزامی است")
            .Must((instance, toUserId) =>
            {
                var fromUserIdProperty = typeof(T).GetProperty("FromUserId");
                if (fromUserIdProperty != null)
                {
                    var fromUserId = (Guid)fromUserIdProperty.GetValue(instance)!;
                    return fromUserId != toUserId;
                }
                return true;
            })
            .WithMessage("کاربر مقصد باید متفاوت از کاربر مبدا باشد");
    }

    #endregion

    #region Pagination Validation

    /// <summary>
    /// Validate pagination parameters
    /// </summary>
    public static IRuleBuilderOptions<T, int> ValidatePage<T>(
        this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0).WithMessage("شماره صفحه باید بزرگتر از صفر باشد");
    }

    /// <summary>
    /// Validate page size
    /// </summary>
    public static IRuleBuilderOptions<T, int> ValidatePageSize<T>(
        this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0).WithMessage("اندازه صفحه باید بزرگتر از صفر باشد")
            .LessThanOrEqualTo(100).WithMessage("اندازه صفحه نباید بیش از 100 باشد");
    }

    #endregion
}