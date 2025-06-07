using System.Numerics;
using WalletPayment.Domain.Enums;
using WalletPayment.Domain.ValueObjects;

namespace WalletPayment.Domain.Common;

/// <summary>
/// Centralized domain validation rules
/// تمام قوانین validation در یک جا
/// </summary>
public static class DomainValidationRules
{
    public static class Iranian
    {
        /// <summary>
        /// اعتبارسنجی کد ملی ایرانی
        /// </summary>
        public static bool IsValidNationalCode(string? nationalCode)
        {
            if (string.IsNullOrWhiteSpace(nationalCode) || nationalCode.Length != 10 || !nationalCode.All(char.IsDigit))
                return false;

            var digits = nationalCode.Select(c => int.Parse(c.ToString())).ToArray();
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

        /// <summary>
        /// اعتبارسنجی شماره کارت ایرانی (الگوریتم Luhn)
        /// </summary>
        public static bool IsValidCardNumber(string? cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length != 16 || !cardNumber.All(char.IsDigit))
                return false;

            int sum = 0;
            bool alternate = false;

            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = int.Parse(cardNumber[i].ToString());

                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit = digit % 10 + digit / 10;
                }

                sum += digit;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }

        /// <summary>
        /// اعتبارسنجی شماره شبا ایرانی
        /// </summary>
        public static bool IsValidShabaNumber(string? shabaNumber)
        {
            if (string.IsNullOrWhiteSpace(shabaNumber))
                return false;

            // Remove spaces and convert to uppercase
            shabaNumber = shabaNumber.Replace(" ", "").ToUpper();

            // Must start with IR and be 26 characters total
            if (!shabaNumber.StartsWith("IR") || shabaNumber.Length != 26)
                return false;

            // Extract the numeric part
            var numericPart = shabaNumber.Substring(2);
            if (!numericPart.All(char.IsDigit))
                return false;

            // IBAN validation using mod-97
            try
            {
                var rearranged = numericPart + "1827"; // IR = 1827
                var checkSum = CalculateMod97(rearranged);
                return checkSum == 1;
            }
            catch
            {
                return false;
            }
        }

        private static int CalculateMod97(string number)
        {
            var remainder = 0;
            foreach (char digit in number)
            {
                remainder = (remainder * 10 + (digit - '0')) % 97;
            }
            return remainder;
        }

        /// <summary>
        /// اعتبارسنجی شماره حساب بانکی
        /// </summary>
        public static bool IsValidAccountNumber(string? accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                return false;

            if (!accountNumber.All(char.IsDigit))
                return false;

            return accountNumber.Length >= 10 && accountNumber.Length <= 20;
        }
    }

    public static class Personal
    {
        /// <summary>
        /// اعتبارسنجی تاریخ تولد
        /// </summary>
        public static bool IsValidBirthDate(DateTime birthDate)
        {
            return birthDate <= DateTime.Today && birthDate >= DateTime.Today.AddYears(-120);
        }

        /// <summary>
        /// اعتبارسنجی شماره پاسپورت
        /// </summary>
        public static bool IsValidPassportNumber(string? passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber))
                return false;

            // Passport should be 6-20 characters, alphanumeric
            return System.Text.RegularExpressions.Regex.IsMatch(passportNumber, @"^[A-Z0-9]{6,20}$");
        }
    }

    public static class Financial
    {
        /// <summary>
        /// اعتبارسنجی مبلغ تراکنش
        /// </summary>
        public static bool IsValidTransactionAmount(Money amount)
        {
            if (amount == null) return false;
            if (amount.Value <= 0) return false;

            // Check minimum amount for IRR
            if (amount.Currency == CurrencyCode.IRR && amount.Value < BusinessRules.Amounts.MinimumTransactionAmount.Value)
                return false;

            // Check maximum amount for IRR
            if (amount.Currency == CurrencyCode.IRR && amount.Value > BusinessRules.Amounts.MaximumSingleTransactionAmount.Value)
                return false;

            return true;
        }

        /// <summary>
        /// اعتبارسنجی دقت ارز
        /// </summary>
        public static bool IsValidCurrencyPrecision(decimal amount, CurrencyCode currency)
        {
            return currency switch
            {
                CurrencyCode.IRR => amount % 1 == 0, // Whole numbers only
                CurrencyCode.USD or CurrencyCode.EUR => Math.Round(amount, 2) == amount, // 2 decimal places
                _ => false
            };
        }

        /// <summary>
        /// بررسی اعتبار مبلغ اعتباری
        /// </summary>
        public static bool IsValidCreditAmount(Money amount)
        {
            if (amount == null) return false;
            if (amount.Value <= 0) return false;
            if (amount.Value > BusinessRules.Credit.MaximumCreditLimit.Value) return false;

            return true;
        }
    }

    public static class Business
    {
        /// <summary>
        /// اعتبارسنجی URL
        /// </summary>
        public static bool IsValidUrl(string? url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// اعتبارسنجی تاریخ سررسید اعتبار
        /// </summary>
        public static bool IsValidCreditDueDate(DateTime dueDate)
        {
            var maxDueDate = DateTime.UtcNow.AddDays(BusinessRules.Credit.MaximumCreditDurationDays);
            return dueDate > DateTime.UtcNow && dueDate <= maxDueDate;
        }

        /// <summary>
        /// بررسی timeout پرداخت
        /// </summary>
        public static bool HasPaymentTimedOut(DateTime paymentInitiatedAt)
        {
            return DateTime.UtcNow > paymentInitiatedAt.AddMinutes(BusinessRules.Payment.PaymentTimeoutMinutes);
        }
    }
}