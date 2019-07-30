using System.Collections.Immutable;
using Bearded.Utilities;

namespace Bearded.TD.Game.Technologies
{
    sealed class Technology
    {
        public Id<Technology> Id;
        public int Cost { get; }
        public ImmutableList<ITechnologyEffect> Effects { get; }

        public Technology(Id<Technology> id, int cost, ImmutableList<ITechnologyEffect> effects)
        {
            Id = id;
            Cost = cost;
            Effects = effects;
        }
    }
}
