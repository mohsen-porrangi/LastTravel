using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace BuildingBlocks.Contracts
{
    /// <summary>
    /// Base Repository for CRUD operations and Query
    /// </summary>
    public interface IRepositoryBase<T, in TKey>
    {
        #region Query Methods (for Performance)

        /// <summary>
        /// get IQueryable without filters - for complicated Query 
        /// you have to use it with ToListAsync()
        /// </summary>
        IQueryable<T> Query();

        /// <summary>
        /// get IQueryable with filters - for complicated Query 
        /// you have to use it with ToListAsync()
        /// </summary>
        IQueryable<T> Query(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// get IQueryable with includes - for complicated Query
        /// you have to use it with ToListAsync()
        /// </summary>
        IQueryable<T> QueryWithIncludes(Func<IQueryable<T>, IIncludableQueryable<T, object>> include);

        #endregion

        #region Read Operations (Safe Methods)

        /// <summary>
        /// get an entity with ID
        /// </summary>
        Task<T?> GetByIdAsync(TKey id, bool track = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// get all entities
        /// use for small tables
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync(bool track = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get entities with condition
        /// </summary>
        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            bool track = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get entities and includes with condition
        /// </summary>
        Task<IEnumerable<T>> FindWithIncludesAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include,
            bool track = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// get first entity that prove the condition
        /// </summary>
        Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            bool track = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// get first entity that prove the condition with include
        /// </summary>
        Task<T?> FirstOrDefaultWithIncludesAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include,
            bool track = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// check if entity is exist
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// count entities
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

        #endregion

        #region Write Operations

        /// <summary>
        /// add a entity
        /// </summary>
        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// add multi entity
        /// </summary>
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// update the entity
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// update multi entity
        /// </summary>
        void UpdateRange(IEnumerable<T> entities);

        /// <summary>
        /// delete entity with ID
        /// </summary>
        Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>
        /// remove an entity
        /// </summary>
        void Delete(T entity);

        /// <summary>
        /// remove multi entity
        /// </summary>
        void DeleteRange(IEnumerable<T> entities);

        #endregion
    }
}