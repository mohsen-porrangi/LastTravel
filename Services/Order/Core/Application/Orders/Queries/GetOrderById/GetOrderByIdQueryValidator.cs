using FluentValidation;

namespace Order.Application.Orders.Queries.GetOrderById;

/// <summary>
/// اعتبارسنجی درخواست دریافت جزئیات سفارش
/// </summary>
public class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("شناسه سفارش الزامی است");

        RuleFor(x => x.Include)
            .IsInEnum()
            .WithMessage("نوع اطلاعات درخواستی معتبر نیست");
    }
}