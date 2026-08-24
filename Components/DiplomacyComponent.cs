using System.Collections.Generic;
using WorldNMilSim.Core;

namespace WorldNMilSim.Components;

public enum RelationStance { War, Neutral, Allied }

public class DiplomacyComponent
{
    private readonly Dictionary<(int, int), RelationStance> _relations = new();

    private static (int, int) Key(Entity a, Entity b) => a.Id < b.Id ? (a.Id, b.Id) : (b.Id, a.Id);

    public RelationStance GetStance(Entity a, Entity b)
    {
        if (a.Id == b.Id) return RelationStance.Allied;
        return _relations.TryGetValue(Key(a, b), out var stance) ? stance : RelationStance.War;
    }

    public void SetStance(Entity a, Entity b, RelationStance stance)
    {
        if (a.Id == b.Id) return;
        _relations[Key(a, b)] = stance;
    }

    public RelationStance CycleStance(Entity a, Entity b)
    {
        var next = GetStance(a, b) switch
        {
            RelationStance.War => RelationStance.Neutral,
            RelationStance.Neutral => RelationStance.Allied,
            _ => RelationStance.War
        };
        SetStance(a, b, next);
        return next;
    }
}