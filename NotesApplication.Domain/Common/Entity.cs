namespace NotesApplication.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected init; }

    public override bool Equals(object? obj)
        => obj is Entity other && other.GetType() == GetType() && Id != Guid.Empty && other.Id == Id;

    public override int GetHashCode()
        => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right) => Equals(left, right);

    public static bool operator !=(Entity? left, Entity? right) => !Equals(left, right);
}
