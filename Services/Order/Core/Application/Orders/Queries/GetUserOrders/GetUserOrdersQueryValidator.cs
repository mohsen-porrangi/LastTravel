using FluentValidation;

namespace Order.Application.Orders.Queries.GetUserOrders;

/// <summary>
/// اعتبارسنجی کامل درخواست دریافت لیست سفارشات
/// </summary>
public class GetUserOrdersQueryValidator : AbstractValidator<GetUserOrdersQuery>
{
    private const int MaxPageSize = 100;
    private const int MinPageSize = 1;
    private const int MaxDateRangeDays = 365;
    private static readonly DateTime MinAllowedDate = DateTime.Today.AddYears(-5);
    private static readonly string[] ValidSortFields = { "createdat", "ordernumber", "amount", "status" };
    private static readonly string[] ValidSortDirections = { "asc", "desc" };

    public GetUserOrdersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("شماره صفحه باید بزرگتر از 0 باشد");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(MinPageSize, MaxPageSize)
            .WithMessage($"اندازه صفحه باید بین {MinPageSize} تا {MaxPageSize} باشد");

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("وضعیت سفارش معتبر نیست");

        RuleFor(x => x.ServiceType)
            .IsInEnum()
            .When(x => x.ServiceType.HasValue)
            .WithMessage("نوع سرویس معتبر نیست");

        RuleFor(x => x.FromDate)
            .GreaterThanOrEqualTo(MinAllowedDate)
            .When(x => x.FromDate.HasValue)
            .WithMessage("تاریخ شروع نمی‌تواند بیش از 5 سال قبل باشد");

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
            .WithMessage("تاریخ پایان باید بعد از تاریخ شروع باشد");

        RuleFor(x => x)
            .Must(HaveValidDateRange)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
            .WithMessage($"بازه زمانی نمی‌تواند بیش از {MaxDateRangeDays} روز باشد");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
            .WithMessage("عبارت جستجو نباید بیش از 100 کاراکتر باشد");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy))
            .WithMessage($"فیلد مرتب‌سازی باید یکی از موارد زیر باشد: {string.Join(", ", ValidSortFields)}");

        RuleFor(x => x.SortDirection)
            .Must(BeValidSortDirection)
            .WithMessage($"جهت مرتب‌سازی باید یکی از موارد زیر باشد: {string.Join(", ", ValidSortDirections)}");
    }

    private static bool HaveValidDateRange(GetUserOrdersQuery query)
    {
        if (!query.FromDate.HasValue || !query.ToDate.HasValue)
            return true;

        var daysDifference = (query.ToDate.Value - query.FromDate.Value).Days;
        return daysDifference <= MaxDateRangeDays;
    }

    private static bool BeValidSortField(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy) ||
               ValidSortFields.Contains(sortBy.ToLower());
    }

    private static bool BeValidSortDirection(string sortDirection)
    {
        return ValidSortDirections.Contains(sortDirection.ToLower());
    }
}