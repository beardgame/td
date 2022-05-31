using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements;

interface IElementalEffect
{
    IElementalPhenomenon Phenomenon { get; }
    TimeSpan Duration { get; }
}
