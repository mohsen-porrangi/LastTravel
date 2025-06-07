using FluentValidation;

namespace Order.Application.Trains.Queries.GetTrainTickets;

/// <summary>
/// اعتبارسنجی درخواست دریافت بلیط‌های قطار
/// </summary>
public class GetTrainTicketsQueryValidator : AbstractValidator<GetTrainTicketsQuery>
{
    public GetTrainTicketsQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("شناسه سفارش الزامی است");
    }
}