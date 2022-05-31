using Bearded.TD.Game.Simulation.Damage;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static partial class OnFire
{
    public readonly record struct Effect(
        HitPoints DamagePerSecond, IDamageSource? DamageSource, TimeSpan Duration) : IElementalEffect
    {
        IElementalPhenomenon IElementalEffect.Phenomenon => phenomenon;
    }
}
