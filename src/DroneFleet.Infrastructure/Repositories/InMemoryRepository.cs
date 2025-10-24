using DroneFleet.Domain.Common;
using DroneFleet.Domain.Repositories;
using System.Collections.Concurrent;

namespace DroneFleet.Infrastructure.Repositories;

/// <summary>
/// Thread-safe in-memory repository implementation identified by a key selector.
/// </summary>
/// <typeparam name="TEntity">Type of entity stored.</typeparam>
/// <typeparam name="TKey">Type of key used to identify entities.</typeparam>
public class InMemoryRepository<TEntity, TKey>(Func<TEntity, TKey> keySelector, IEqualityComparer<TKey>? comparer = null) : IRepository<TEntity, TKey>
    where TEntity : class
    where TKey : notnull
{
    private readonly Func<TEntity, TKey> _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
    private readonly ConcurrentDictionary<TKey, TEntity> _items = new(comparer ?? EqualityComparer<TKey>.Default);

    public Result<TEntity> Add(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var key = _keySelector(entity);
        if (!_items.TryAdd(key, entity))
        {
            return Result<TEntity>.Failure("An entity with the same key already exists.", ResultCodes.DuplicateKey);
        }

        return Result<TEntity>.Success(entity);
    }

    public Result<TEntity> Upsert(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var key = _keySelector(entity);
        _items.AddOrUpdate(key, entity, (_, _) => entity);
        return Result<TEntity>.Success(entity);
    }

    public Result<TEntity> GetById(TKey key)
    {
        if (_items.TryGetValue(key, out var entity))
        {
            return Result<TEntity>.Success(entity);
        }

        return Result<TEntity>.Failure("Entity not found.", ResultCodes.NotFound);
    }

    public Result Remove(TKey key)
    {
        return _items.TryRemove(key, out _)
            ? Result.Success()
            : Result.Failure("Entity not found.", ResultCodes.NotFound);
    }

    public IReadOnlyCollection<TEntity> List()
    {
        return _items.Values.ToArray();
    }
}
