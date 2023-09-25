using System;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static partial class OnFire
{
    private static readonly IElementalPhenomenon phenomenon = new Phenomenon();

    private sealed class Phenomenon : IElementalPhenomenon
    {
        public Type EffectType => typeof(Effect);

        public IElementalPhenomenon.IScope NewScope(GameObject target) => new Scope(target);
    }
}
