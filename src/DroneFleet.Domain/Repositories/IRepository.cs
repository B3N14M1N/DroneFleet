using DroneFleet.Domain.Common;

namespace DroneFleet.Domain.Repositories;

/// <summary>
/// Defines the contract for a repository that stores entities identified by a key.
/// </summary>
/// <typeparam name="TEntity">Type of the entity stored.</typeparam>
/// <typeparam name="TKey">Type of the entity key.</typeparam>
public interface IRepository<TEntity, TKey>
    where TEntity : class
{
    /// <summary>
    /// Attempts to add the provided entity. Returns a duplicate failure when the key already exists.
    /// </summary>
    Result<TEntity> Add(TEntity entity);

    /// <summary>
    /// Adds or replaces the entity identified by the provided key.
    /// </summary>
    Result<TEntity> Upsert(TEntity entity);

    /// <summary>
    /// Gets the entity identified by the provided key.
    /// </summary>
    Result<TEntity> GetById(TKey key);

    /// <summary>
    /// Removes the entity identified by the provided key.
    /// </summary>
    Result Remove(TKey key);

    /// <summary>
    /// Retrieves a snapshot of all entities currently stored.
    /// </summary>
    IReadOnlyCollection<TEntity> List();
}
