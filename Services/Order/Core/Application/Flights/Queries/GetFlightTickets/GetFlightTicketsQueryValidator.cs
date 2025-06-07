using FluentValidation;

namespace Order.Application.Flights.Queries.GetFlightTickets;

/// <summary>
/// اعتبارسنجی درخواست دریافت بلیط‌های پرواز
/// </summary>
public class GetFlightTicketsQueryValidator : AbstractValidator<GetFlightTicketsQuery>
{
    public GetFlightTicketsQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("شناسه سفارش الزامی است");
    }
}