using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static partial class Shocked
{
    public readonly record struct Effect(double MovementPenalty, TimeSpan Duration) : IElementalEffect
    {
        IElementalPhenomenon IElementalEffect.Phenomenon => phenomenon;
    }
}
