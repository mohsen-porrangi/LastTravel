using PaymentGateway.API.Models;
using System.Text.Json;

namespace PaymentGateway.API.Gateways;

public class ZarinPalGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ZarinPalGateway> _logger;

    public ZarinPalGateway(HttpClient httpClient, IConfiguration configuration, ILogger<ZarinPalGateway> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public PaymentGatewayType GatewayType => PaymentGatewayType.ZarinPal;

    public async Task<PaymentGatewayResult> CreatePaymentAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var merchantId = _configuration["PaymentGateways:ZarinPal:MerchantId"];
            var isTestMode = bool.Parse(_configuration["PaymentGateways:ZarinPal:IsTestMode"] ?? "false");

            var apiUrl = isTestMode
                ? "https://sandbox.zarinpal.com/pg/rest/WebGate/PaymentRequest.json"
                : "https://www.zarinpal.com/pg/rest/WebGate/PaymentRequest.json";

            var paymentUrl = isTestMode
                ? "https://sandbox.zarinpal.com/pg/StartPay/"
                : "https://www.zarinpal.com/pg/StartPay/";

            var amountInToman = ConvertToToman(request.Amount, request.Currency);

            var zarinRequest = new
            {
                MerchantID = merchantId,
                Amount = amountInToman,
                Description = request.Description,
                CallbackURL = request.CallbackUrl,
                Mobile = request.MobileNumber ?? "",
                Email = request.Email ?? ""
            };

            var response = await _httpClient.PostAsJsonAsync(apiUrl, zarinRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("ZarinPal API call failed with status: {StatusCode}", response.StatusCode);
                return new PaymentGatewayResult(false, ErrorMessage: "خطا در ارتباط با درگاه پرداخت", ErrorCode: PaymentErrorCode.ConnectionFailed);
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var zarinResponse = JsonSerializer.Deserialize<ZarinPalResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (zarinResponse?.Status != 100)
            {
                var errorMessage = GetZarinPalErrorMessage(zarinResponse?.Status ?? -1);
                _logger.LogError("ZarinPal payment creation failed: {Status} - {Message}", zarinResponse?.Status, errorMessage);
                return new PaymentGatewayResult(false, ErrorMessage: errorMessage, ErrorCode: MapZarinPalErrorCode(zarinResponse?.Status ?? -1));
            }

            var fullPaymentUrl = $"{paymentUrl}{zarinResponse.Authority}";
            _logger.LogInformation("ZarinPal payment created successfully. Authority: {Authority}", zarinResponse.Authority);

            return new PaymentGatewayResult(true, zarinResponse.Authority, fullPaymentUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while creating ZarinPal payment");
            return new PaymentGatewayResult(false, ErrorMessage: "خطای سیستمی", ErrorCode: PaymentErrorCode.Unknown);
        }
    }

    public async Task<PaymentGatewayVerificationResult> VerifyPaymentAsync(PaymentGatewayVerificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.Status != "OK" && request.Status != "success")
            {
                _logger.LogWarning("ZarinPal payment cancelled or failed. Authority: {Authority}, Status: {Status}", request.Authority, request.Status);
                return new PaymentGatewayVerificationResult(false, ErrorMessage: "پرداخت توسط کاربر لغو شد", ErrorCode: PaymentErrorCode.CanceledByUser);
            }

            var merchantId = _configuration["PaymentGateways:ZarinPal:MerchantId"];
            var isTestMode = bool.Parse(_configuration["PaymentGateways:ZarinPal:IsTestMode"] ?? "false");

            var apiUrl = isTestMode
                ? "https://sandbox.zarinpal.com/pg/rest/WebGate/PaymentVerification.json"
                : "https://www.zarinpal.com/pg/rest/WebGate/PaymentVerification.json";

            var amountInToman = ConvertToToman(request.Amount, CurrencyCode.IRR);

            var verifyRequest = new
            {
                MerchantID = merchantId,
                Authority = request.Authority,
                Amount = amountInToman
            };

            var response = await _httpClient.PostAsJsonAsync(apiUrl, verifyRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("ZarinPal verification API call failed with status: {StatusCode}", response.StatusCode);
                return new PaymentGatewayVerificationResult(false, ErrorMessage: "خطا در تأیید پرداخت", ErrorCode: PaymentErrorCode.ConnectionFailed);
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var zarinResponse = JsonSerializer.Deserialize<ZarinPalVerificationResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (zarinResponse?.Status != 100)
            {
                var errorMessage = GetZarinPalErrorMessage(zarinResponse?.Status ?? -1);
                _logger.LogError("ZarinPal payment verification failed: {Status} - {Message}", zarinResponse?.Status, errorMessage);
                return new PaymentGatewayVerificationResult(false, ErrorMessage: errorMessage, ErrorCode: MapZarinPalErrorCode(zarinResponse?.Status ?? -1));
            }

            _logger.LogInformation("ZarinPal payment verified successfully. RefID: {RefID}", zarinResponse.RefID);
            return new PaymentGatewayVerificationResult(true, zarinResponse.RefID.ToString(), request.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while verifying ZarinPal payment");
            return new PaymentGatewayVerificationResult(false, ErrorMessage: "خطای سیستمی در تأیید پرداخت", ErrorCode: PaymentErrorCode.Unknown);
        }
    }

    public async Task<PaymentGatewayRefundResult> RefundPaymentAsync(PaymentGatewayRefundRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("ZarinPal refund requested for ReferenceId: {ReferenceId}, Amount: {Amount}", request.ReferenceId, request.Amount);
        return new PaymentGatewayRefundResult(false, ErrorMessage: "استرداد خودکار برای ZarinPal پشتیبانی نمی‌شود", ErrorCode: PaymentErrorCode.RefundNotAllowed);
    }

    private static decimal ConvertToToman(decimal amount, CurrencyCode currency)
    {
        return currency switch
        {
            CurrencyCode.IRR => Math.Round(amount / 10),
            _ => throw new ArgumentException($"Currency {currency} not supported for ZarinPal")
        };
    }

    private static string GetZarinPalErrorMessage(int status) => status switch
    {
        -1 => "اطلاعات ارسال شده ناقص است",
        -2 => "IP یا مرچنت کد صحیح نیست",
        -3 => "با توجه به محدودیت‌های شاپرک، امکان پرداخت با رقم درخواست شده میسر نیست",
        -4 => "سطح تأیید پذیرنده پایین‌تر از سطح نقره‌ای است",
        -11 => "درخواست مورد نظر یافت نشد",
        -12 => "امکان ویرایش درخواست میسر نیست",
        -21 => "هیچ نوع عملیات مالی برای این تراکنش یافت نشد",
        -22 => "تراکنش ناموفق بود",
        -33 => "رقم تراکنش با رقم پرداخت شده مطابقت ندارد",
        -34 => "سقف تقسیم تراکنش از لحاظ تعداد یا رقم عبور شده است",
        -40 => "اجازه دسترسی به متد مربوطه وجود ندارد",
        -41 => "اطلاعات ارسال شده مربوط به AdditionalData غیرمعتبر است",
        -42 => "مدت زمان معتبر طول عمر شناسه پرداخت باید بین 30 دقیقه تا 45 روز باشد",
        -54 => "درخواست مورد نظر آرشیو شده است",
        101 => "عملیات پرداخت موفق بوده و قبلاً انجام شده است",
        _ => $"خطای ناشناخته با کد {status}"
    };

    private static PaymentErrorCode MapZarinPalErrorCode(int status) => status switch
    {
        -1 => PaymentErrorCode.InvalidAmount,
        -2 => PaymentErrorCode.MerchantNotFound,
        -3 => PaymentErrorCode.InvalidAmount,
        -11 => PaymentErrorCode.AuthorityNotFound,
        -21 => PaymentErrorCode.AuthorityNotFound,
        -22 => PaymentErrorCode.GatewayError,
        -33 => PaymentErrorCode.InvalidAmount,
        -34 => PaymentErrorCode.DuplicateTransaction,
        101 => PaymentErrorCode.DuplicateTransaction,
        _ => PaymentErrorCode.GatewayError
    };

    private class ZarinPalResponse
    {
        public int Status { get; set; }
        public string Authority { get; set; } = string.Empty;
    }

    private class ZarinPalVerificationResponse
    {
        public int Status { get; set; }
        public long RefID { get; set; }
    }
}