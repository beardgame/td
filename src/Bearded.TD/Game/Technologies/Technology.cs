using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bearded.TD.Game.Technologies
{
    [Obsolete]
    sealed class Technology : ITechnologyBlueprint
    {
        public string Id { get; }
        public string Name { get; }
        public int Cost { get; }
        public IEnumerable<ITechnologyUnlock> Unlocks { get; }

        public Technology(string id, string name, int cost, ImmutableList<ITechnologyUnlock> effects)
        {
            Id = id;
            Name = name;
            Cost = cost;
            Unlocks = effects;
        }
    }
}
