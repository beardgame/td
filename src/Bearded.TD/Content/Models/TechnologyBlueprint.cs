using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Technologies;

namespace Bearded.TD.Content.Models
{
    sealed class TechnologyBlueprint : ITechnologyBlueprint
    {
        public string Id { get; }
        public string Name { get; }
        public int Cost { get; }
        public IEnumerable<ITechnologyUnlock> Unlocks { get; }

        public TechnologyBlueprint(string id, string name, int cost, IEnumerable<ITechnologyUnlock> unlocks)
        {
            Id = id;
            Name = name;
            Cost = cost;
            Unlocks = unlocks?.ToImmutableList() ?? ImmutableList<ITechnologyUnlock>.Empty;
        }
    }
}
