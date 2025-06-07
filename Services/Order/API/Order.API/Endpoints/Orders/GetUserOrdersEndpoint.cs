using BuildingBlocks.Contracts;
using BuildingBlocks.Exceptions;
using Carter;
using FluentValidation;
using MediatR;
using Order.API.Extensions;
using Order.API.Models.Common;
using Order.API.Models.Order;
using Order.Domain.Enums;

namespace Order.API.Endpoints.Orders;

public class GetUserOrdersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders", async (
            HttpContext context,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken ct) =>
        {
            try
            {
                var request = ExtractRequestFromQuery(context.Request.Query);
                var query = request.ToQuery();
                var result = await sender.Send(query, ct);
                var response = result.ToApiResponse();

                context.Response.Headers.Add("X-Total-Count", result.TotalCount.ToString());
                context.Response.Headers.Add("X-Page-Number", result.PageNumber.ToString());
                context.Response.Headers.Add("X-Page-Size", result.Items.Count.ToString());

                return Results.Ok(response);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new ValidationErrorResponse
                {
                    Message = "اطلاعات ورودی نامعتبر است",
                    Errors = ex.Errors.Select(e => new ValidationError
                    {
                        Field = e.PropertyName,
                        Message = e.ErrorMessage,
                        AttemptedValue = e.AttemptedValue?.ToString()
                    }).ToList()
                });
            }
            catch (BadRequestException ex)
            {
                return Results.BadRequest(new ErrorResponse
                {
                    Message = ex.Message,
                    Details = ex.Details,
                    Type = "BadRequest"
                });
            }
            catch (UnauthorizedDomainException ex)
            {
                return Results.Unauthorized();
            }
            catch (ForbiddenDomainException ex)
            {
                return Results.Forbid();
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new ErrorResponse
                {
                    Message = ex.Message,
                    Type = "NotFound"
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while processing your request",
                    statusCode: 500,
                    type: "InternalServerError"
                );
            }
        })
        .WithName("GetUserOrders")
        .WithSummary("دریافت لیست سفارشات کاربر")
        .WithDescription("دریافت لیست سفارشات کاربر با قابلیت‌های پیشرفته فیلترینگ، جستجو، مرتب‌سازی و صفحه‌بندی")
        .WithOpenApi()
        .Produces<GetUserOrdersResponse>(StatusCodes.Status200OK)
        .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<Microsoft.AspNetCore.Mvc.ProblemDetails>(StatusCodes.Status500InternalServerError)
        .WithTags("Orders")
        .RequireAuthorization();
    }

    private static GetUserOrdersRequest ExtractRequestFromQuery(IQueryCollection query)
    {
        return new GetUserOrdersRequest
        {
            PageNumber = TryParseInt(query["pageNumber"], 1),
            PageSize = TryParseInt(query["pageSize"], 10),
            Status = TryParseEnum<OrderStatus>(query["status"]),
            ServiceType = TryParseEnum<ServiceType>(query["serviceType"]),
            FromDate = TryParseDateTime(query["fromDate"]),
            ToDate = TryParseDateTime(query["toDate"]),
            SearchTerm = query["searchTerm"].FirstOrDefault(),
            SortBy = query["sortBy"].FirstOrDefault(),
            SortDirection = query["sortDirection"].FirstOrDefault() ?? "desc"
        };
    }

    private static int TryParseInt(string? value, int defaultValue)
    {
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    private static T? TryParseEnum<T>(string? value) where T : struct, Enum
    {
        return Enum.TryParse<T>(value, true, out var result) ? result : null;
    }

    private static DateTime? TryParseDateTime(string? value)
    {
        return DateTime.TryParse(value, out var result) ? result : null;
    }
}