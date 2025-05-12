using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

interface IElementalEffect
{
    TimeSpan Duration { get; }

    interface IScope
    {
        void ApplyTick(Instant now);
    }
}

interface IElementalEffect<in TEffect> : IElementalEffect
    where TEffect : IElementalEffect<TEffect>
{
    new interface IScope : IElementalEffect.IScope
    {
        void Adopt(TEffect effect, Instant now);
    }

    IScope NewScope(GameObject target);
}
