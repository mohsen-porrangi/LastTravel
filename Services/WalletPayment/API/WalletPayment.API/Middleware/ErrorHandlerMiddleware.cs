using BuildingBlocks.Exceptions;
using FluentValidation;
using System.Net;
using System.Text.Json;
using WalletPayment.Domain.Exceptions;

namespace WalletPayment.API.Middleware;

/// <summary>
/// Global error handling middleware
/// </summary>
public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // Use traditional if-else instead of switch expression for better type inference
        object response;
        int statusCode;

        if (exception is ValidationException validationEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            response = new
            {
                title = "Validation Error",
                detail = "One or more validation errors occurred",
                status = statusCode,
                errors = GetValidationErrors(validationEx)
            };
        }
        else if (exception is BadRequestException badRequestEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            response = new
            {
                title = "Bad Request",
                detail = badRequestEx.Message,
                status = statusCode,
                details = badRequestEx.Details
            };
        }
        else if (exception is NotFoundException notFoundEx)
        {
            statusCode = (int)HttpStatusCode.NotFound;
            response = new
            {
                title = "Not Found",
                detail = notFoundEx.Message,
                status = statusCode
            };
        }
        else if (exception is UnauthorizedDomainException)
        {
            statusCode = (int)HttpStatusCode.Unauthorized;
            response = new
            {
                title = "Unauthorized",
                detail = "You are not authorized to perform this action",
                status = statusCode
            };
        }
        else if (exception is ForbiddenDomainException)
        {
            statusCode = (int)HttpStatusCode.Forbidden;
            response = new
            {
                title = "Forbidden",
                detail = "Access to this resource is forbidden",
                status = statusCode
            };
        }
        else if (exception is ConflictDomainException conflictEx)
        {
            statusCode = (int)HttpStatusCode.Conflict;
            response = new
            {
                title = "Conflict",
                detail = conflictEx.Message,
                status = statusCode,
                details = conflictEx.Details
            };
        }
        else if (exception is ServiceCommunicationException)
        {
            statusCode = (int)HttpStatusCode.ServiceUnavailable;
            response = new
            {
                title = "Service Communication Error",
                detail = "خطا در ارتباط با سرویس خارجی",
                status = statusCode
            };
        }
        else if (exception is InsufficientBalanceException insufficientEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            response = new
            {
                title = "Insufficient Balance",
                detail = "موجودی کافی نیست",
                status = statusCode,
                walletId = insufficientEx.WalletId,
                requestedAmount = insufficientEx.RequestedAmount,
                availableBalance = insufficientEx.AvailableBalance
            };
        }
        else if (exception is WalletNotFoundException walletNotFoundEx)
        {
            statusCode = (int)HttpStatusCode.NotFound;
            response = new
            {
                title = "Wallet Not Found",
                detail = "کیف پول یافت نشد",
                status = statusCode,
                userId = walletNotFoundEx.UserId,
                walletId = walletNotFoundEx.WalletId
            };
        }
        else if (exception is DuplicateWalletException duplicateWalletEx)
        {
            statusCode = (int)HttpStatusCode.Conflict;
            response = new
            {
                title = "Duplicate Wallet",
                detail = "کیف پول تکراری",
                status = statusCode,
                userId = duplicateWalletEx.UserId
            };
        }
        else if (exception is InvalidBankAccountException bankAccountEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            response = new
            {
                title = "Invalid Bank Account",
                detail = bankAccountEx.Message,
                status = statusCode,
                details = bankAccountEx.Details
            };
        }
        else if (exception is InvalidCurrencyException currencyEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            response = new
            {
                title = "Invalid Currency",
                detail = $"ارز {currencyEx.CurrencyCode} پشتیبانی نمی‌شود",
                status = statusCode,
                currencyCode = currencyEx.CurrencyCode
            };
        }
        else if (exception is InvalidCurrencyAccountException currencyAccountEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            response = new
            {
                title = "Invalid Currency Account",
                detail = $"حساب ارزی {currencyAccountEx.Currency} یافت نشد",
                status = statusCode,
                currency = currencyAccountEx.Currency.ToString()
            };
        }
        else if (exception is InvalidTransactionException transactionEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            response = new
            {
                title = "Invalid Transaction",
                detail = transactionEx.Message,
                status = statusCode,
                details = transactionEx.Details
            };
        }
        else if (exception is DuplicateTransactionException duplicateTransactionEx)
        {
            statusCode = (int)HttpStatusCode.Conflict;
            response = new
            {
                title = "Duplicate Transaction",
                detail = "تراکنش تکراری",
                status = statusCode,
                transactionNumber = duplicateTransactionEx.TransactionNumber
            };
        }
        else if (exception is UnauthorizedAccessException)
        {
            statusCode = (int)HttpStatusCode.Unauthorized;
            response = new
            {
                title = "Unauthorized",
                detail = "دسترسی غیرمجاز",
                status = statusCode
            };
        }
        else if (exception is ArgumentException argumentEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            response = new
            {
                title = "Invalid Argument",
                detail = argumentEx.Message,
                status = statusCode,
                parameterName = argumentEx.ParamName
            };
        }
        else if (exception is InvalidOperationException invalidOpEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            response = new
            {
                title = "Invalid Operation",
                detail = invalidOpEx.Message,
                status = statusCode
            };
        }
        else if (exception is NotSupportedException notSupportedEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            response = new
            {
                title = "Not Supported",
                detail = notSupportedEx.Message,
                status = statusCode
            };
        }
        else if (exception is TimeoutException)
        {
            statusCode = (int)HttpStatusCode.RequestTimeout;
            response = new
            {
                title = "Timeout",
                detail = "عملیات به دلیل انقضای زمان متوقف شد",
                status = statusCode
            };
        }
        else
        {
            // Default case for unknown exceptions
            statusCode = (int)HttpStatusCode.InternalServerError;
            response = new
            {
                title = "Internal Server Error",
                detail = "خطای داخلی سرور",
                status = statusCode,
                traceId = context.TraceIdentifier
            };
        }

        context.Response.StatusCode = statusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static Dictionary<string, string[]> GetValidationErrors(ValidationException validationException)
    {
        return validationException.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());
    }
}
