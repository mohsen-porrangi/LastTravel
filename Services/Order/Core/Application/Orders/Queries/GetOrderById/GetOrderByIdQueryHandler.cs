using AutoMapper;
using BuildingBlocks.Contracts;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Application.Common.DTOs;
using Order.Application.Common.Extensions;
using Order.Domain.Contracts;
using System.Diagnostics;

namespace Order.Application.Orders.Queries.GetOrderById;

/// <summary>
/// Handler برای دریافت جزئیات کامل یک سفارش خاص با قابلیت Include انتخابی
/// </summary>
public class GetOrderByIdQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IMapper mapper,
    ILogger<GetOrderByIdQueryHandler> logger) : IQueryHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly ActivitySource _activitySource = new("Order.Application");

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("GetOrderById");
        var stopwatch = Stopwatch.StartNew();

        var currentUserId = currentUserService.GetCurrentUserId();

        // ✅ Structured logging với correlation
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["UserId"] = currentUserId,
            ["OrderId"] = request.OrderId,
            ["IncludeFlags"] = request.Include.ToString()
        });

        logger.LogDebug("Fetching order {OrderId} for user {UserId} with includes: {Include}",
            request.OrderId, currentUserId, request.Include);

        try
        {
            // ✅ بررسی security access control
            await ValidateOrderAccess(request.OrderId, currentUserId, cancellationToken);

            // ✅ ساخت query با include strategy
            var includeFunc = BuildIncludeStrategy(request.Include);

            // ✅ واکشی سفارش با strategy pattern
            var order = await ExecuteOrderQuery(request.OrderId, currentUserId, includeFunc, cancellationToken);

            // ✅ لود اطلاعات اضافی به صورت موازی
            await LoadAdditionalDataAsync(order, request.Include, cancellationToken);

            // ✅ mapping با استفاده از extensions
            var orderDto = await MapToOrderDtoAsync(order, cancellationToken);

            // ✅ Performance logging
            stopwatch.Stop();
            logger.LogInformation("Successfully retrieved order {OrderId} for user {UserId} in {ElapsedMs}ms",
                request.OrderId, currentUserId, stopwatch.ElapsedMilliseconds);

            // ✅ Activity tags برای monitoring
            activity?.SetTag("order.id", request.OrderId.ToString());
            activity?.SetTag("order.itemsCount", orderDto.Items.Count);
            activity?.SetTag("performance.duration_ms", stopwatch.ElapsedMilliseconds);

            return orderDto;
        }
        catch (Exception ex) when (ex is not NotFoundException and not UnauthorizedDomainException and not ForbiddenDomainException)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Unexpected error while fetching order {OrderId} for user {UserId} in {ElapsedMs}ms",
                request.OrderId, currentUserId, stopwatch.ElapsedMilliseconds);

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw new InternalServerException($"خطا در دریافت جزئیات سفارش {request.OrderId}");
        }
    }

    /// <summary>
    /// اعتبارسنجی دسترسی کاربر به سفارش
    /// </summary>
    private async Task ValidateOrderAccess(Guid orderId, Guid userId, CancellationToken cancellationToken)
    {
        var hasAccess = await unitOfWork.Orders.ExistsAsync(
            o => o.Id == orderId && o.UserId == userId && !o.IsDeleted,
            cancellationToken);

        if (!hasAccess)
        {
            logger.LogWarning("User {UserId} attempted to access unauthorized order {OrderId}", userId, orderId);
            throw new NotFoundException("سفارش یافت نشد یا شما اجازه دسترسی به آن را ندارید");
        }
    }

    /// <summary>
    /// ساخت استراتژی Include بر اساس فلگ‌های درخواست
    /// </summary>
    private static Func<IQueryable<Domain.Entities.Order>, IQueryable<Domain.Entities.Order>>? BuildIncludeStrategy(IncludeOrder includeFlags)
    {
        if (includeFlags == IncludeOrder.None)
            return null;

        return query =>
        {
            if (includeFlags.HasFlag(IncludeOrder.OrderFlights))
                query = query.Include(o => o.OrderFlights);

            if (includeFlags.HasFlag(IncludeOrder.OrderTrains))
                query = query.Include(o => o.OrderTrains);

            if (includeFlags.HasFlag(IncludeOrder.OrderTrainCarTransports))
                query = query.Include(o => o.OrderTrainCarTransports);

            if (includeFlags.HasFlag(IncludeOrder.WalletTransactions))
                query = query.Include(o => o.WalletTransactions);

            if (includeFlags.HasFlag(IncludeOrder.StatusHistory))
                query = query.Include(o => o.StatusHistories);

            return query;
        };
    }

    /// <summary>
    /// اجرای query سفارش با include استراتژی
    /// </summary>
    private async Task<Domain.Entities.Order> ExecuteOrderQuery(
        Guid orderId,
        Guid userId,
        Func<IQueryable<Domain.Entities.Order>, IQueryable<Domain.Entities.Order>>? includeFunc,
        CancellationToken cancellationToken)
    {
        var query = unitOfWork.Orders.Query(o => o.Id == orderId && o.UserId == userId && !o.IsDeleted);

        if (includeFunc != null)
            query = includeFunc(query);

        return await query.FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("سفارش یافت نشد");
    }

    /// <summary>
    /// لود اطلاعات اضافی که نیاز به query جداگانه دارند
    /// </summary>
    private async Task LoadAdditionalDataAsync(Domain.Entities.Order order, IncludeOrder includeFlags, CancellationToken cancellationToken)
    {
        var loadTasks = new List<Task>();

        // ✅ اگر WalletTransactions include نشده باشد اما درخواست شده باشد
        if (includeFlags.HasFlag(IncludeOrder.WalletTransactions) && !order.WalletTransactions.Any())
        {
            loadTasks.Add(LoadWalletTransactionsAsync(order, cancellationToken));
        }

        // ✅ اگر StatusHistory include نشده باشد اما درخواست شده باشد  
        if (includeFlags.HasFlag(IncludeOrder.StatusHistory) && !order.StatusHistories.Any())
        {
            loadTasks.Add(LoadStatusHistoriesAsync(order, cancellationToken));
        }

        if (loadTasks.Any())
        {
            await Task.WhenAll(loadTasks);
        }
    }

    /// <summary>
    /// لود تراکنش‌های کیف پول
    /// </summary>
    private async Task LoadWalletTransactionsAsync(Domain.Entities.Order order, CancellationToken cancellationToken)
    {
        var transactions = await unitOfWork.Orders.Query()
            .Where(o => o.Id == order.Id)
            .SelectMany(o => o.WalletTransactions)
            .ToListAsync(cancellationToken);

        // Manual population برای navigation property
        foreach (var transaction in transactions)
        {
            order.WalletTransactions.Add(transaction);
        }
    }

    /// <summary>
    /// لود تاریخچه وضعیت‌ها
    /// </summary>
    private async Task LoadStatusHistoriesAsync(Domain.Entities.Order order, CancellationToken cancellationToken)
    {
        var histories = await unitOfWork.Orders.Query()
            .Where(o => o.Id == order.Id)
            .SelectMany(o => o.StatusHistories)
            .OrderBy(h => h.CreatedAt)
            .ToListAsync(cancellationToken);

        // Manual population برای navigation property
        foreach (var history in histories)
        {
            order.StatusHistories.Add(history);
        }
    }

    /// <summary>
    /// تبدیل Order entity به OrderDto
    /// </summary>
    private async Task<OrderDto> MapToOrderDtoAsync(Domain.Entities.Order order, CancellationToken cancellationToken)
    {
        // ✅ Base mapping
        var orderDto = mapper.Map<OrderDto>(order);

        // ✅ Map items using extensions
        var items = order.MapToOrderItems(mapper);

        return orderDto with { Items = items };
    }
}