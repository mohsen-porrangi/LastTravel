using FluentValidation;
using System.Numerics;

namespace BuildingBlocks.Exceptions;

public static class ValidationHelpers
{
    public static IRuleBuilderOptions<T, string?> ValidationIranianNationalCode<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Matches(@"^\d{10}$").WithMessage("کد ملی باید ۱۰ رقم باشد.")
            .NotEmpty().WithMessage("کد ملی الزامی است.")
            .Must(IsValidIranianNationalCode)
            .WithMessage("کد ملی نامعتبر است.");
    }
    private static bool IsValidIranianNationalCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 10 || !code.All(char.IsDigit))
            return false;

        var digits = code.Select(c => int.Parse(c.ToString())).ToArray();
        var check = digits[9];
        var sum = 0;

        for (int i = 0; i < 9; i++)
        {
            sum += digits[i] * (10 - i);
        }

        var remainder = sum % 11;
        return remainder < 2 && check == remainder ||
               remainder >= 2 && check == 11 - remainder;
    }
    public static bool IsValidIranCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length != 16 || !cardNumber.All(char.IsDigit))
            return false;

        var digits = cardNumber.Select(c => int.Parse(c.ToString())).ToArray();
        int sum = 0;
        for (int i = 0; i < 16; i++)
        {
            int d = digits[i];
            if (i % 2 == 0)
            {
                d *= 2;
                if (d > 9) d -= 9;
            }
            sum += d;
        }
        return sum % 10 == 0;
    }
    public static bool IsValidIban(string iban)
    {
        // تبدیل "IRxxxxxxxxxxxxxxxxxxxxxxxx" به فرمت قابل بررسی
        if (string.IsNullOrWhiteSpace(iban) || iban.Length != 26)
            return false;

        iban = iban.ToUpper().Replace(" ", "");

        // جابجایی ۴ کاراکتر اول به انتهای رشته
        var rearranged = iban.Substring(4) + iban.Substring(0, 4);

        // تبدیل کاراکترها به اعداد (A = 10, B = 11, ..., Z = 35)
        var numericIban = "";
        foreach (char ch in rearranged)
        {
            if (char.IsDigit(ch))
                numericIban += ch;
            else if (char.IsLetter(ch))
                numericIban += (ch - 'A' + 10).ToString();
            else
                return false; // کاراکتر نامعتبر
        }

        // بررسی Mod-97
        try
        {
            var ibanNumber = BigInteger.Parse(numericIban);
            return ibanNumber % 97 == 1;
        }
        catch
        {
            return false;
        }
    }
    public static IRuleBuilderOptions<T, DateTime> ValidateBirthDate<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("تاریخ تولد الزامی است")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("تاریخ تولد نمی‌تواند در آینده باشد");
    }
    public static IRuleBuilderOptions<T, string?> ValidatePassportNumber<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("شماره پاسپورت الزامی است")
            .Matches(@"^[A-Z0-9]{6,20}$")
            .WithMessage("شماره پاسپورت باید بین ۶ تا ۲۰ کاراکتر و شامل حروف بزرگ و ارقام باشد");
    }
}
