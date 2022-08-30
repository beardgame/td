using System;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static partial class Shocked
{
    private static readonly IElementalPhenomenon phenomenon = new Phenomenon();

    private sealed class Phenomenon : IElementalPhenomenon
    {
        public Type EffectType => typeof(Effect);

        public IElementalPhenomenon.IScope NewScope() => new Scope();
    }
}
