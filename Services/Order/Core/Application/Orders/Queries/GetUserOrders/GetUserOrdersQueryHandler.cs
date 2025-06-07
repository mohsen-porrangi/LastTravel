using AutoMapper;
using AutoMapper.QueryableExtensions;
using BuildingBlocks.Contracts;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.Extensions.Logging;
using Order.Application.Common.DTOs;
using Order.Application.Common.Models;
using Order.Domain.Contracts;
using System.Diagnostics;

namespace Order.Application.Orders.Queries.GetUserOrders;

/// <summary>
/// Handler برای دریافت لیست سفارشات کاربر با فیلتر، مرتب‌سازی و صفحه‌بندی بهینه
/// </summary>
public class GetUserOrdersQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IMapper mapper,
    ILogger<GetUserOrdersQueryHandler> logger) : IQueryHandler<GetUserOrdersQuery, PaginatedList<OrderSummaryDto>>
{
    private readonly ActivitySource _activitySource = new("Order.Application");

    public async Task<PaginatedList<OrderSummaryDto>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("GetUserOrders");
        var stopwatch = Stopwatch.StartNew();

        var userId = currentUserService.GetCurrentUserId();

        // ✅ Structured logging با correlation
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["UserId"] = userId,
            ["PageNumber"] = request.PageNumber,
            ["PageSize"] = request.PageSize,
            ["HasFilters"] = HasActiveFilters(request)
        });

        logger.LogDebug("Starting to fetch user orders with filters: {@Request}", new
        {
            request.PageNumber,
            request.PageSize,
            request.Status,
            request.ServiceType,
            request.FromDate,
            request.ToDate,
            SearchTermLength = request.SearchTerm?.Length ?? 0
        });

        try
        {
            // ✅ Pre-flight checks
            await ValidateUserAccess(userId, cancellationToken);

            // ✅ Build optimized query
            var query = BuildBaseQuery(userId);
            query = ApplyFilters(query, request);
            query = ApplySorting(query, request);

            // ✅ Execute with metrics
            var mappedQuery = query.ProjectTo<OrderSummaryDto>(mapper.ConfigurationProvider);

            var result = await ExecuteQueryWithMetrics(mappedQuery, request, cancellationToken);

            // ✅ Enhanced logging
            stopwatch.Stop();
            logger.LogInformation("Successfully retrieved {Count} orders out of {Total} for user {UserId} in {ElapsedMs}ms",
                result.Items.Count, result.TotalCount, userId, stopwatch.ElapsedMilliseconds);

            // ✅ Add activity tags for monitoring
            activity?.SetTag("orders.count", result.Items.Count);
            activity?.SetTag("orders.total", result.TotalCount);
            activity?.SetTag("orders.page", result.PageNumber);
            activity?.SetTag("performance.duration_ms", stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex) when (ex is not BadRequestException and not NotFoundException and not UnauthorizedDomainException)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Unexpected error while fetching orders for user {UserId} in {ElapsedMs}ms",
                userId, stopwatch.ElapsedMilliseconds);

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw new InternalServerException("خطا در دریافت لیست سفارشات");
        }
    }

    /// <summary>
    /// بررسی دسترسی کاربر (می‌تواند شامل rate limiting باشد)
    /// </summary>
    private async Task ValidateUserAccess(Guid userId, CancellationToken cancellationToken)
    {
        // ✅ Optional: Check if user exists and is active
        // var userExists = await userManagementService.IsUserActiveAsync(userId);
        // if (!userExists)
        //     throw new UnauthorizedDomainException("کاربر غیرفعال یا موجود نیست");

        // ✅ Optional: Rate limiting check
        // await rateLimitingService.CheckUserLimitAsync(userId, "GetOrders", cancellationToken);

        await Task.CompletedTask; // Placeholder for future validations
    }

    /// <summary>
    /// ساخت query پایه با بهینه‌سازی
    /// </summary>
    private IQueryable<Domain.Entities.Order> BuildBaseQuery(Guid userId)
    {
        return unitOfWork.Orders.Query(x =>
            x.UserId == userId &&
            !x.IsDeleted);
    }

    /// <summary>
    /// اعمال فیلترهای کسب‌وکار
    /// </summary>
    private static IQueryable<Domain.Entities.Order> ApplyFilters(
        IQueryable<Domain.Entities.Order> query,
        GetUserOrdersQuery request)
    {
        // ✅ Status filter
        if (request.Status.HasValue)
        {
            query = query.Where(o => o.LastStatus == request.Status.Value);
        }

        // ✅ Service type filter
        if (request.ServiceType.HasValue)
        {
            query = query.Where(o => o.ServiceType == request.ServiceType.Value);
        }

        // ✅ Date range filters (optimized)
        if (request.FromDate.HasValue)
        {
            var fromDate = request.FromDate.Value.Date;
            query = query.Where(o => o.CreatedAt >= fromDate);
        }

        if (request.ToDate.HasValue)
        {
            var toDate = request.ToDate.Value.Date.AddDays(1);
            query = query.Where(o => o.CreatedAt < toDate);
        }

        // ✅ Search filter (intelligent)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim();
            query = ApplySearchFilter(query, searchTerm);
        }

        return query;
    }

    /// <summary>
    /// اعمال فیلتر جستجوی هوشمند
    /// </summary>
    private static IQueryable<Domain.Entities.Order> ApplySearchFilter(
        IQueryable<Domain.Entities.Order> query,
        string searchTerm)
    {
        // ✅ Order number exact match (highest priority)
        if (searchTerm.StartsWith("ORD", StringComparison.OrdinalIgnoreCase))
        {
            return query.Where(o => o.OrderNumber == searchTerm.ToUpper());
        }

        // ✅ Partial order number match
        return query.Where(o => o.OrderNumber.Contains(searchTerm.ToUpper()));
    }

    /// <summary>
    /// اعمال مرتب‌سازی
    /// </summary>
    private static IQueryable<Domain.Entities.Order> ApplySorting(
        IQueryable<Domain.Entities.Order> query,
        GetUserOrdersQuery request)
    {
        // ✅ Default sorting: newest first with deterministic secondary sort
        return request.SortBy?.ToLower() switch
        {
            "ordernumber" => request.SortDirection == "asc"
                ? query.OrderBy(o => o.OrderNumber).ThenByDescending(o => o.CreatedAt)
                : query.OrderByDescending(o => o.OrderNumber).ThenByDescending(o => o.CreatedAt),

            "amount" => request.SortDirection == "asc"
                ? query.OrderBy(o => o.FullAmount).ThenByDescending(o => o.CreatedAt)
                : query.OrderByDescending(o => o.FullAmount).ThenByDescending(o => o.CreatedAt),

            "status" => request.SortDirection == "asc"
                ? query.OrderBy(o => o.LastStatus).ThenByDescending(o => o.CreatedAt)
                : query.OrderByDescending(o => o.LastStatus).ThenByDescending(o => o.CreatedAt),

            // Default: newest first
            _ => query.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id)
        };
    }

    /// <summary>
    /// اجرای query با metrics
    /// </summary>
    private async Task<PaginatedList<OrderSummaryDto>> ExecuteQueryWithMetrics(
        IQueryable<OrderSummaryDto> mappedQuery,
        GetUserOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var queryStopwatch = Stopwatch.StartNew();

        try
        {
            var result = await PaginatedList<OrderSummaryDto>.CreateAsync(
                mappedQuery, request.PageNumber, request.PageSize);

            queryStopwatch.Stop();

            // ✅ Performance monitoring
            if (queryStopwatch.ElapsedMilliseconds > 1000) // > 1 second
            {
                logger.LogWarning("Slow query detected: {ElapsedMs}ms for page {PageNumber} with {TotalCount} total items",
                    queryStopwatch.ElapsedMilliseconds, request.PageNumber, result.TotalCount);
            }

            return result;
        }
        catch (Exception)
        {
            queryStopwatch.Stop();
            logger.LogError("Query execution failed after {ElapsedMs}ms", queryStopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// بررسی وجود فیلترهای فعال
    /// </summary>
    private static bool HasActiveFilters(GetUserOrdersQuery request)
    {
        return request.Status.HasValue ||
               request.ServiceType.HasValue ||
               request.FromDate.HasValue ||
               request.ToDate.HasValue ||
               !string.IsNullOrWhiteSpace(request.SearchTerm);
    }
}