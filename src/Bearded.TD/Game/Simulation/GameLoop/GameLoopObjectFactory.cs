using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameLoop;

static class GameLoopObjectFactory
{
    public static GameObject CreateSpawnLocation(GameState game, Id<SpawnLocation> id, Tile tile)
    {
        var obj = ComponentGameObjectFactory.CreateWithoutRenderer(game, null, Level.GetPosition(tile).WithZ(0.U()));
        obj.AddComponent(new SpawnLocation(id, tile));
        return obj;
    }

    public static GameObject CreateEnemyPathIndicator(GameState game, Tile tile)
    {
        var obj = ComponentGameObjectFactory.CreateWithoutRenderer(game, null, Level.GetPosition(tile).WithZ(0.U()));
        obj.AddComponent(new EnemyPathIndicator(tile));
        return obj;
    }
}
