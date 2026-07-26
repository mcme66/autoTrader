namespace FinanceAnalysis.Domain.Common;

/// <summary>
/// Base class for persisted entities, providing identity-based equality.
/// </summary>
/// <typeparam name="TId">The type of the primary key.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : struct, IEquatable<TId>
{
    public TId Id { get; protected set; }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        // Transient entities (default id) are only ever equal by reference.
        if (Id.Equals(default) || other.Id.Equals(default))
        {
            return false;
        }

        return GetType() == other.GetType() && Id.Equals(other.Id);
    }

    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
