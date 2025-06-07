using BuildingBlocks.Domain;
using BuildingBlocks.Enums;
using Order.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Order.Domain.Entities;

[NotMapped]
public abstract class OrderItem : BaseEntity<long>
{
    public Guid OrderId { get; protected set; }
    public string FirstNameEn { get; protected set; } = string.Empty;
    public string LastNameEn { get; protected set; } = string.Empty;
    public string FirstNameFa { get; protected set; } = string.Empty;
    public string LastNameFa { get; protected set; } = string.Empty;
    public DateTime BirthDate { get; protected set; }
    public AgeGroup AgeGroup { get; protected set; }
    public bool IsIranian { get; protected set; }
    public string? NationalCode { get; protected set; }
    public string? PassportNumber { get; protected set; }
    public Gender Gender { get; protected set; }
    public int SourceCode { get; protected set; }
    public int DestinationCode { get; protected set; }
    public string SourceName { get; protected set; } = string.Empty;
    public string DestinationName { get; protected set; } = string.Empty;
    public TicketDirection TicketDirection { get; protected set; }
    public DateTime DepartureTime { get; protected set; }
    public DateTime ArrivalTime { get; protected set; }
    public decimal BasePrice { get; protected set; }
    public decimal Tax { get; protected set; }
    public decimal Fee { get; protected set; }
    public decimal TotalPrice => BasePrice + Tax + Fee;

    // Ticket information
    public string? PNR { get; protected set; }
    public string? TicketNumber { get; protected set; }
    public DateTime? IssueDate { get; protected set; }
    public string? SeatNumber { get; protected set; }

    protected OrderItem() { }

    protected OrderItem(
        Guid orderId,
        string firstNameEn, string lastNameEn,
        string firstNameFa, string lastNameFa,
        DateTime birthDate, Gender gender,
        bool isIranian, string? nationalCode, string? passportNumber,
        int sourceCode, int destinationCode,
        string sourceName, string destinationName,
        TicketDirection ticketDirection,
        DateTime departureTime, DateTime arrivalTime)
    {
        OrderId = orderId;
        FirstNameEn = firstNameEn;
        LastNameEn = lastNameEn;
        FirstNameFa = firstNameFa;
        LastNameFa = lastNameFa;
        BirthDate = birthDate;
        Gender = gender;
        IsIranian = isIranian;
        NationalCode = nationalCode;
        PassportNumber = passportNumber;
        SourceCode = sourceCode;
        DestinationCode = destinationCode;
        SourceName = sourceName;
        DestinationName = destinationName;
        TicketDirection = ticketDirection;
        DepartureTime = departureTime;
        ArrivalTime = arrivalTime;
        AgeGroup = CalculateAgeGroup(birthDate);
        CreatedAt = DateTime.UtcNow;
    }

    public void IssueTicket(string pnr, string ticketNumber, string? seatNumber = null)
    {
        PNR = pnr;
        TicketNumber = ticketNumber;
        SeatNumber = seatNumber;
        IssueDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPricing(decimal basePrice, decimal tax, decimal fee)
    {
        BasePrice = basePrice;
        Tax = tax;
        Fee = fee;
        UpdatedAt = DateTime.UtcNow;
    }

    private AgeGroup CalculateAgeGroup(DateTime birthDate)
    {
        var age = DateTime.Today.Year - birthDate.Year;
        if (birthDate.Date > DateTime.Today.AddYears(-age)) age--;

        //  قوانین سنی استاندارد صنعت هوانوردی
        return age switch
        {
            < 2 => AgeGroup.Infant,   // زیر 2 سال
            >= 2 and < 12 => AgeGroup.Child,  // 2 تا 12 سال
            _ => AgeGroup.Adult       // بالای 12 سال
        };
    }
}