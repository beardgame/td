using System;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.GameObjects.Parameters;

public abstract class TemplateBase
{
    public bool AddModification(AttributeType type, Modification modification)
    {
        throw new InvalidOperationException("Cannot modify attributes on immutable template.");
    }

    public bool AddModificationWithId(AttributeType type, ModificationWithId modification)
    {
        throw new InvalidOperationException("Cannot modify attributes on immutable template.");
    }

    public bool UpdateModification(AttributeType type, Id<Modification> id, Modification modification)
    {
        throw new InvalidOperationException("Cannot modify attributes on immutable template.");
    }

    public bool RemoveModification(AttributeType type, Id<Modification> id)
    {
        throw new InvalidOperationException("Cannot modify attributes on immutable template.");
    }
}
