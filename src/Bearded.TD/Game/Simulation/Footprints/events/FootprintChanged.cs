using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Game.Simulation.Footprints
{
    readonly struct FootprintChanged : IComponentEvent
    {
        public PositionedFootprint NewFootprint { get; }

        public FootprintChanged(PositionedFootprint newFootprint)
        {
            NewFootprint = newFootprint;
        }
    }
}
