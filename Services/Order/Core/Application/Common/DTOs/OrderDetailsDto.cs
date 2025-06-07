using Order.Domain.Enums;

namespace Order.Application.Common.DTOs;

/// <summary>
/// جزئیات کامل سفارش
/// </summary>
public record OrderDetailsDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public ServiceType ServiceType { get; init; }
    public string ServiceTypeName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public OrderStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public int PassengerCount { get; init; }
    public bool HasReturn { get; init; }
    public DateTime CreatedAt { get; init; }

    // جزئیات بلیط‌ها
    public List<TicketDetailsDto> Tickets { get; init; } = new();

    // تاریخچه وضعیت
    public List<StatusHistoryDto> StatusHistory { get; init; } = new();

    // تراکنش‌های مالی
    public List<WalletTransactionDto> Transactions { get; init; } = new();
}

public record TicketDetailsDto
{
    public long Id { get; init; }
    public string PassengerNameFa { get; init; } = string.Empty;
    public string PassengerNameEn { get; init; } = string.Empty;
    public string ServiceNumber { get; init; } = string.Empty; // Flight/Train number
    public TicketDirection Direction { get; init; }
    public string DirectionName { get; init; } = string.Empty;
    public DateTime DepartureTime { get; init; }
    public DateTime ArrivalTime { get; init; }
    public string SourceName { get; init; } = string.Empty;
    public string DestinationName { get; init; } = string.Empty;
    public string? TicketNumber { get; init; }
    public string? PNR { get; init; }
    public string? SeatNumber { get; init; }
    public DateTime? IssueDate { get; init; }
    public decimal BasePrice { get; init; }
    public decimal Tax { get; init; }
    public decimal Fee { get; init; }
    public decimal TotalPrice { get; init; }
    public string TicketType { get; init; } = string.Empty; // Flight/Train/CarTransport
}

public record StatusHistoryDto
{
    public OrderStatus FromStatus { get; init; }
    public string FromStatusName { get; init; } = string.Empty;
    public OrderStatus ToStatus { get; init; }
    public string ToStatusName { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record WalletTransactionDto
{
    public long TransactionId { get; init; }
    public TransactionType Type { get; init; }
    public string TypeName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime CreatedAt { get; init; }
}