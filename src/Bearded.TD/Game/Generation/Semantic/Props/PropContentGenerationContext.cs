using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.Props;

sealed class PropContentGenerationContext
{
    private readonly LevelGenerationCommandAccumulator commandAccumulator;

    public PropContentGenerationContext(LevelGenerationCommandAccumulator commandAccumulator)
    {
        this.commandAccumulator = commandAccumulator;
    }

    public void PlaceGameObject(IGameObjectBlueprint blueprint, Position3 position, Direction2 direction)
    {
        commandAccumulator.PlaceGameObject(blueprint, position, direction);
    }
}
