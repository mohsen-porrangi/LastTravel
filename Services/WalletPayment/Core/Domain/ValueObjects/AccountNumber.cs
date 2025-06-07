namespace WalletPayment.Domain.ValueObjects;

/// <summary>
/// Value Object برای شماره حساب بانکی
/// </summary>
public record AccountNumber
{
    public string Value { get; }

    private AccountNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// ایجاد شماره حساب از رشته
    /// </summary>
    public static AccountNumber FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("شماره حساب نمی‌تواند خالی باشد", nameof(value));

        // حداقل اعتبارسنجی - می‌تواند در آینده بهبود یابد
        if (value.Length < 10 || value.Length > 20)
            throw new ArgumentException("شماره حساب باید بین 10 تا 20 کاراکتر باشد", nameof(value));

        if (!value.All(char.IsDigit))
            throw new ArgumentException("شماره حساب باید فقط شامل عدد باشد", nameof(value));

        return new AccountNumber(value);
    }

    /// <summary>
    /// Account ID value object
    /// </summary>
    public record AccountId
    {
        public Guid Value { get; }

        private AccountId(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("Account ID cannot be empty", nameof(value));

            Value = value;
        }

        public static AccountId Create() => new(Guid.NewGuid());
        public static AccountId From(Guid value) => new(value);

        public static implicit operator Guid(AccountId accountId) => accountId.Value;
        public static implicit operator AccountId(Guid value) => From(value);

        public override string ToString() => Value.ToString();
    }


    public override string ToString() => Value;
}