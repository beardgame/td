using System.Collections.Immutable;
using Bearded.Utilities;

namespace Bearded.TD.Game.Technologies
{
    sealed class Technology
    {
        public Id<Technology> Id { get; }
        public string Name { get; }
        public int Cost { get; }
        public ImmutableList<ITechnologyEffect> Effects { get; }

        public Technology(Id<Technology> id, string name, int cost, ImmutableList<ITechnologyEffect> effects)
        {
            Id = id;
            Name = name;
            Cost = cost;
            Effects = effects;
        }
    }
}
