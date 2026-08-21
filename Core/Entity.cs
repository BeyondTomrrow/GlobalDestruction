using System;

namespace WorldNMilSim.Core;

public readonly struct Entity : IEquatable<Entity>
{
    public readonly int Id;
    internal Entity(int id) => Id = id;

    public bool Equals(Entity other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is Entity e && Equals(e);
    public override int GetHashCode() => Id;
    public static bool operator ==(Entity a, Entity b) => a.Equals(b);
    public static bool operator !=(Entity a, Entity b) => !a.Equals(b);
    public override string ToString() => $"Entity({Id})";
}