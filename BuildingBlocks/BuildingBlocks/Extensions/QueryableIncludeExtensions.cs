using Microsoft.EntityFrameworkCore.Query;

namespace BuildingBlocks.Extensions;

public static class QueryableIncludeExtensions
{
    public static Func<IQueryable<T>, IIncludableQueryable<T, object>> CombineIncludes<T>(
        this Func<IQueryable<T>, IIncludableQueryable<T, object>> first,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> second)
        where T : class
    {
        return q => second(first(q));
    }
}
