using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

interface IElementalPhenomenon
{
    public Type EffectType { get; }
    public IScope NewScope(GameObject target);

    interface IScope
    {
        void ApplyTick(Instant now);
    }

    interface IScope<in T> : IScope where T : IElementalEffect
    {
        void Adopt(T effect, Instant now);
    }
}
