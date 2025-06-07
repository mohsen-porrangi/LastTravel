using FluentValidation;

namespace Order.Application.Orders.Commands.CancelOrder;

/// <summary>
/// اعتبارسنجی Command لغو سفارش
/// </summary>
public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("شناسه سفارش الزامی است");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("دلیل لغو الزامی است")
            .MinimumLength(5).WithMessage("دلیل لغو باید حداقل 5 کاراکتر باشد")
            .MaximumLength(500).WithMessage("دلیل لغو نباید بیش از 500 کاراکتر باشد");
    }
}