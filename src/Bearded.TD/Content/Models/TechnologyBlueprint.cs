using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Technologies;

namespace Bearded.TD.Content.Models;

sealed class TechnologyBlueprint : ITechnologyBlueprint
{
    public ModAwareId Id { get; }
    public string Name { get; }
    public TechnologyBranch Branch { get; }
    public IEnumerable<ITechnologyUnlock> Unlocks { get; }
    public IEnumerable<ITechnologyBlueprint> RequiredTechs { get; }

    public TechnologyBlueprint(
        ModAwareId id,
        string name,
        TechnologyBranch branch,
        IEnumerable<ITechnologyUnlock>? unlocks,
        IEnumerable<ITechnologyBlueprint>? requiredTechs)
    {
        Id = id;
        Name = name;
        Branch = branch;
        Unlocks = unlocks?.ToImmutableArray() ?? ImmutableArray<ITechnologyUnlock>.Empty;
        RequiredTechs = requiredTechs?.ToImmutableArray() ?? ImmutableArray<ITechnologyBlueprint>.Empty;
    }
}
