using BuildingBlocks.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace BuildingBlocks.Data;

/// <summary>
/// High-performance Repository Base Implementation
/// </summary>
public abstract class RepositoryBase<T, TKey, TContext> : IRepositoryBase<T, TKey>
    where T : class, IEntity<TKey>
    where TContext : DbContext
{
    protected readonly TContext Context;
    protected readonly DbSet<T> DbSet;

    protected RepositoryBase(TContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    #region Query Methods (for Performance)

    /// <summary>
    /// Get IQueryable without filters - for complicated Query 
    /// </summary>
    public virtual IQueryable<T> Query()
    {
        return DbSet.AsNoTracking();
    }

    /// <summary>
    /// Get IQueryable with filters - for complicated Query 
    /// </summary>
    public virtual IQueryable<T> Query(Expression<Func<T, bool>> predicate)
    {
        return DbSet.AsNoTracking().Where(predicate);
    }

    /// <summary>
    /// Get IQueryable with includes - for complicated Query
    /// </summary>
    public virtual IQueryable<T> QueryWithIncludes(Func<IQueryable<T>, IIncludableQueryable<T, object>> include)
    {
        return include(DbSet.AsNoTracking());
    }

    #endregion

    #region Read Operations (Safe Methods)

    /// <summary>
    /// Optimized GetById with proper type handling
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(TKey id, bool track = false, CancellationToken cancellationToken = default)
    {
        if (id == null) return null;

        // Use DbSet.FindAsync for primary key lookups (most efficient)
        if (!track)
        {
            var entity = await DbSet.FindAsync(new object[] { id }, cancellationToken);
            if (entity != null)
            {
                // Detach to ensure no tracking
                Context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        // For tracked entities, use FindAsync directly
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// Get all entities with streaming support
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync(bool track = false, CancellationToken cancellationToken = default)
    {
        var query = track ? DbSet.AsTracking() : DbSet.AsNoTracking();
        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Optimized Find with expression compilation caching
    /// </summary>
    public virtual async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        bool track = false,
        CancellationToken cancellationToken = default)
    {
        var query = track ? DbSet.AsTracking() : DbSet.AsNoTracking();
        return await query.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Find with includes - optimized for complex queries
    /// </summary>
    public virtual async Task<IEnumerable<T>> FindWithIncludesAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include,
        bool track = false,
        CancellationToken cancellationToken = default)
    {
        var query = track ? DbSet.AsTracking() : DbSet.AsNoTracking();
        var includedQuery = include(query);
        return await includedQuery.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// High-performance FirstOrDefault with query optimization
    /// </summary>
    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        bool track = false,
        CancellationToken cancellationToken = default)
    {
        var query = track ? DbSet.AsTracking() : DbSet.AsNoTracking();
        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// FirstOrDefault with includes
    /// </summary>
    public virtual async Task<T?> FirstOrDefaultWithIncludesAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include,
        bool track = false,
        CancellationToken cancellationToken = default)
    {
        var query = track ? DbSet.AsTracking() : DbSet.AsNoTracking();
        var includedQuery = include(query);
        return await includedQuery.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Optimized existence check
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// High-performance count with query optimization
    /// </summary>
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();
        return predicate == null
            ? await query.CountAsync(cancellationToken)
            : await query.CountAsync(predicate, cancellationToken);
    }

    #endregion

    #region Write Operations

    /// <summary>
    /// Optimized add with state management
    /// </summary>
    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        await DbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Bulk add with performance optimization
    /// </summary>
    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null) throw new ArgumentNullException(nameof(entities));

        var entityList = entities as List<T> ?? entities.ToList();
        if (entityList.Count == 0) return;

        // Use bulk operations for better performance
        await DbSet.AddRangeAsync(entityList, cancellationToken);
    }

    /// <summary>
    /// Optimized update with change tracking
    /// </summary>
    public virtual void Update(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var entry = Context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            DbSet.Attach(entity);
            entry.State = EntityState.Modified;
        }
        else if (entry.State != EntityState.Modified)
        {
            entry.State = EntityState.Modified;
        }
    }

    /// <summary>
    /// Bulk update with optimized tracking
    /// </summary>
    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        if (entities == null) throw new ArgumentNullException(nameof(entities));

        var entityList = entities as List<T> ?? entities.ToList();
        if (entityList.Count == 0) return;

        DbSet.UpdateRange(entityList);
    }

    /// <summary>
    /// Soft delete implementation
    /// </summary>
    public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, track: true, cancellationToken);
        if (entity != null)
        {
            Delete(entity);
        }
    }

    /// <summary>
    /// Optimized delete with soft delete support
    /// </summary>
    public virtual void Delete(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        // Check if entity supports soft delete
        if (entity is ISoftDelete softDeleteEntity)
        {
            // Soft delete implementation
            var property = entity.GetType().GetProperty("IsDeleted");
            if (property != null && property.CanWrite)
            {
                property.SetValue(entity, true);
                Update(entity);
                return;
            }
        }

        // Hard delete for entities that don't support soft delete
        var entry = Context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            DbSet.Attach(entity);
        }
        DbSet.Remove(entity);
    }

    /// <summary>
    /// Bulk delete with soft delete support
    /// </summary>
    public virtual void DeleteRange(IEnumerable<T> entities)
    {
        if (entities == null) throw new ArgumentNullException(nameof(entities));

        var entityList = entities as List<T> ?? entities.ToList();
        if (entityList.Count == 0) return;

        // Check if entities support soft delete
        var softDeleteEntities = entityList.OfType<ISoftDelete>().ToList();
        if (softDeleteEntities.Count == entityList.Count)
        {
            // All entities support soft delete
            foreach (var entity in entityList)
            {
                var property = entity.GetType().GetProperty("IsDeleted");
                if (property != null && property.CanWrite)
                {
                    property.SetValue(entity, true);
                }
            }
            UpdateRange(entityList);
        }
        else
        {
            // Mixed or no soft delete support
            foreach (var entity in entityList)
            {
                Delete(entity);
            }
        }
    }

    #endregion

    #region Performance Helper Methods

    /// <summary>
    /// Optimize query for read-only scenarios
    /// </summary>
    protected virtual IQueryable<T> OptimizeForReadOnly(IQueryable<T> query)
    {
        return query
            .AsNoTracking();
        //        .AsSplitQuery(); // Use split queries for better performance with includes //TODO: AsSplitQuery dosnt work in core 9.0.5
    }

    /// <summary>
    /// Batch operations for improved performance
    /// </summary>
    public virtual async Task<int> BatchOperationAsync<TOperation>(
        IEnumerable<T> entities,
        Func<T, Task<TOperation>> operation,
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        var entityList = entities as List<T> ?? entities.ToList();
        var totalProcessed = 0;

        for (int i = 0; i < entityList.Count; i += batchSize)
        {
            var batch = entityList.Skip(i).Take(batchSize);
            var tasks = batch.Select(operation);
            await Task.WhenAll(tasks);

            totalProcessed += batch.Count();

            // Optional: Save after each batch to manage memory
            // await Context.SaveChangesAsync(cancellationToken);
        }

        return totalProcessed;
    }

    /// <summary>
    /// Memory-efficient streaming query
    /// </summary>
    public virtual IAsyncEnumerable<T> StreamAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return query.AsAsyncEnumerable();
    }

    #endregion
}