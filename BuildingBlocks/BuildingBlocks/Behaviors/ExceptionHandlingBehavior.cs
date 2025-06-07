using BuildingBlocks.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Behaviors;

public class ExceptionHandlingBehavior<TRequest, TResponse>(
    ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        try
        {
            logger.LogDebug("Handling request {RequestName}", requestName);

            var response = await next();

            logger.LogDebug("Handled request {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            // خطاهای دامنه‌ای ما را دوباره پرتاب می‌کنیم (آنها قبلاً به درستی مدیریت شده‌اند)
            if (ex is BadRequestException ||
                ex is UnauthorizedDomainException ||
                ex is ForbiddenDomainException ||
                ex is NotFoundException ||
                ex is ServiceCommunicationException ||
                ex is InternalServerException)
            {
                logger.LogWarning(ex, "Domain exception in request {RequestName}", requestName);
                throw;
            }

            // خطاهای اعتبارسنجی را دوباره پرتاب می‌کنیم (آنها توسط ValidationBehavior مدیریت شده‌اند)
            if (ex is FluentValidation.ValidationException)
            {
                logger.LogWarning(ex, "Validation exception in request {RequestName}", requestName);
                throw;
            }

            // سایر خطاها را پردازش می‌کنیم
            logger.LogError(ex, "Unhandled exception for request {RequestName}", requestName);

            // اضافه کردن اطلاعات بیشتر به استثنا
            string errorDetail = $"Error handling {requestName}";
            try
            {
                if (request != null)
                {
                    // توجه: سعی می‌کنیم درخواست را سریالایز کنیم، اما اگر خطا رخ داد، آن را نادیده می‌گیریم
                    errorDetail += $" with data: {System.Text.Json.JsonSerializer.Serialize(request)}";
                }
            }
            catch (Exception serializeEx)
            {
                logger.LogWarning(serializeEx, "Could not serialize request for error logging");
            }

            // استثنای کنترل نشده را به یک استثنای دامنه‌ای تبدیل می‌کنیم
            throw new InternalServerException(
                "An error occurred processing the request",
                $"{ex.GetType().Name}: {ex.Message} | {errorDetail}");
        }
    }
}