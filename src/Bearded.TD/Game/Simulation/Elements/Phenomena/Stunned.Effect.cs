using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static partial class Stunned
{
    public readonly record struct Effect(TimeSpan Duration) : IElementalEffect
    {
        IElementalPhenomenon IElementalEffect.Phenomenon => phenomenon;
    }
}
