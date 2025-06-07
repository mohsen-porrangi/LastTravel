using System.Linq.Expressions;
namespace UserManagement.API.Endpoints.Admin.GetUserByCondition
{
    internal sealed class GetUserByConditionQueryHandler(IUserRepository repository)
        : IQueryHandler<GetUserByConditionQuery, GetUserByConditionResult>
    {
        public async Task<GetUserByConditionResult> Handle(GetUserByConditionQuery query, CancellationToken cancellationToken)
        {
            // ساخت شرط بر اساس پارامترهای جستجو
            Expression<Func<User, bool>> predicate = user => true;

            if (!string.IsNullOrWhiteSpace(query.Name))
                predicate = predicate.And(user => user.Name.Contains(query.Name));

            if (!string.IsNullOrWhiteSpace(query.Family))
                predicate = predicate.And(user => user.Family.Contains(query.Family));

            if (!string.IsNullOrWhiteSpace(query.Mobile))
                predicate = predicate.And(user => user.MasterIdentity.Mobile.Contains(query.Mobile));

            if (query.IsActive.HasValue)
                predicate = predicate.And(user => user.MasterIdentity.IsActive == query.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(query.NationalCode))
                predicate = predicate.And(user => user.NationalCode == query.NationalCode);

            // دریافت تعداد کل نتایج
            var totalCount = (await repository.FindAsync(predicate, track: false)).Count();

            // دریافت کاربران بر اساس شرط و صفحه‌بندی
            var users = await repository.FindWithIncludesAsync(predicate, i => i.Include(u => u.MasterIdentity), false);
            var pagedUsers = users
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);

            // تبدیل به DTO
            var result = pagedUsers.Select(user => new UserDto(
                user.Id,
                user.Name,
                user.Family,
                user.MasterIdentity.Email,
                user.MasterIdentity.Mobile,
                user.MasterIdentity.IsActive,
                user.NationalCode
            ));

            return new GetUserByConditionResult(
                result,
                totalCount,
                query.Page,
                query.PageSize
            );
        }
    }

    // Extension method برای ترکیب Expression ها
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            var parameter = Expression.Parameter(typeof(T));
            var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
            var leftExpression = leftVisitor.Visit(left.Body);
            var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
            var rightExpression = rightVisitor.Visit(right.Body);
            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(leftExpression, rightExpression),
                parameter);
        }

        private class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                    return _newValue;
                return base.Visit(node);
            }
        }
    }
}