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
        public IEnumerable<ITechnologyEffect> Effects { get; }

        public TechnologyBlueprint(string id, string name, int cost, IEnumerable<ITechnologyEffect> effects)
        {
            Id = id;
            Name = name;
            Cost = cost;
            Effects = effects?.ToImmutableList() ?? ImmutableList<ITechnologyEffect>.Empty;
        }
    }
}
