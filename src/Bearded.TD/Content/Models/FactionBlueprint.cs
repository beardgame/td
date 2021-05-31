using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Content.Models
{
    sealed class FactionBlueprint : IFactionBlueprint
    {
        public ModAwareId Id { get; }
        private readonly ImmutableArray<IFactionBehaviorFactory<Faction>> behaviorParameters;

        public IEnumerable<FactionBehavior<Faction>> GetBehaviors() => behaviorParameters.Select(f => f.Create());

        public FactionBlueprint(ModAwareId id, IEnumerable<IFactionBehaviorFactory<Faction>> behaviorParameters)
        {
            Id = id;
            this.behaviorParameters = behaviorParameters.ToImmutableArray();
        }
    }
}
