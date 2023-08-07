using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Game.Generation;

readonly struct AccumulateBiomes : IGlobalEvent
{
    private readonly Accumulator<IBiome> accumulator;

    public AccumulateBiomes(Accumulator<IBiome> accumulator)
    {
        this.accumulator = accumulator;
    }

    public void ContributeBiome(IBiome biome)
    {
        accumulator.Contribute(biome);
    }
}
