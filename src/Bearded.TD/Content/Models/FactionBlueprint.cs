using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Content.Models;

sealed class FactionBlueprint : IFactionBlueprint
{
    public ExternalId<Faction> Id { get; }
    public string? Name { get; }
    public Color? Color { get; }

    private readonly ImmutableArray<IFactionBehaviorFactory> behaviorParameters;

    public IEnumerable<IFactionBehavior> GetBehaviors() => behaviorParameters.Select(f => f.Create());

    public FactionBlueprint(
        ExternalId<Faction> id,
        string? name,
        Color? color,
        IEnumerable<IFactionBehaviorFactory> behaviorParameters)
    {
        Id = id;
        Name = name;
        Color = color;
        this.behaviorParameters = behaviorParameters.ToImmutableArray();
    }

    ModAwareId IBlueprint.Id => ModAwareId.Invalid;
}
