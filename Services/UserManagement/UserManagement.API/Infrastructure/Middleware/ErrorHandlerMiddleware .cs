using BuildingBlocks.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.API.Infrastructure.Middleware;

public class ErrorHandlerMiddleware(ILogger<ErrorHandlerMiddleware> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Error occurred: {ExceptionMessage}, Time: {Time}",
            exception.Message,
            DateTime.UtcNow);

        context.Response.ContentType = "application/problem+json";

        var apiError = CreateApiError(exception, context);
        context.Response.StatusCode = apiError.StatusCode;

        // تبدیل ApiError به ProblemDetails
        var problemDetails = new ProblemDetails
        {
            Title = apiError.Title,
            Detail = apiError.Detail,
            Status = apiError.StatusCode,
            Instance = context.Request.Path
        };

        foreach (var extension in apiError.Extensions)
        {
            problemDetails.Extensions.Add(extension.Key, extension.Value);
        }

        if (apiError is ValidationApiError validationError)
        {
            var validationProblemDetails = new ValidationProblemDetails(validationError.Errors)
            {
                Title = validationError.Title,
                Detail = validationError.Detail,
                Status = validationError.StatusCode,
                Instance = context.Request.Path
            };

            foreach (var extension in validationError.Extensions)
            {
                validationProblemDetails.Extensions.Add(extension.Key, extension.Value);
            }

            await context.Response.WriteAsJsonAsync(validationProblemDetails, cancellationToken);
        }
        else
        {
            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        }

        return true;
    }

    private ApiError CreateApiError(Exception exception, HttpContext context)
    {
        ApiError apiError = exception switch
        {
            BadRequestException ex => new ApiError(
                ex.GetType().Name,
                ex.Message,
                StatusCodes.Status400BadRequest),

            ValidationException ex => CreateValidationApiError(ex),

            UnauthorizedDomainException ex => new ApiError(
                ex.GetType().Name,
                ex.Message,
                StatusCodes.Status401Unauthorized),

            ForbiddenDomainException ex => new ApiError(
                ex.GetType().Name,
                ex.Message,
                StatusCodes.Status403Forbidden),

            NotFoundException ex => new ApiError(
                ex.GetType().Name,
                ex.Message,
                StatusCodes.Status404NotFound),

            ServiceCommunicationException ex => new ApiError(
                ex.GetType().Name,
                ex.Message,
                StatusCodes.Status502BadGateway),

            InternalServerException ex => new ApiError(
                ex.GetType().Name,
                ex.Message,
                StatusCodes.Status500InternalServerError),

            _ => new ApiError(
                "InternalServerError",
                "An error occurred while processing your request",
                StatusCodes.Status500InternalServerError)
        };

        apiError.Extensions.Add("traceId", context.TraceIdentifier);

        if (exception is BadRequestException badRequestEx && !string.IsNullOrEmpty(badRequestEx.Details))
        {
            apiError.Extensions.Add("additionalDetail", badRequestEx.Details);
        }

        if (exception is InternalServerException internalEx && !string.IsNullOrEmpty(internalEx.Details))
        {
            apiError.Extensions.Add("additionalDetail", internalEx.Details);
        }

        return apiError;
    }

    private ValidationApiError CreateValidationApiError(ValidationException exception)
    {
        var apiError = new ValidationApiError(
            "ValidationError",
            "One or more validation errors occurred",
            StatusCodes.Status400BadRequest);

        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                group => string.IsNullOrEmpty(group.Key) ? "_" : group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray()
            );

        foreach (var error in errors)
        {
            apiError.Errors.Add(error.Key, error.Value);
        }

        return apiError;
    }
}