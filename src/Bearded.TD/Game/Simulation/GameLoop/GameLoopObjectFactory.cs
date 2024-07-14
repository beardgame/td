using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameLoop;

static class GameLoopObjectFactory
{
    public static GameObject CreateSpawnLocation(Id<SpawnLocation> id, Tile tile)
    {
        var obj = GameObjectFactory.CreateWithoutRenderer(null, Level.GetPosition(tile).WithZ(0.U()));
        obj.AddComponent(new SpawnLocation(id, tile));
        return obj;
    }

    public static GameObject CreateSpawnIndicator(
        GameObject owner, Tile tile, out IFutureEnemySpawnIndicator futureEnemySpawnIndicator)
    {
        var blueprint = owner.Game.Meta.Blueprints.GameObjects[ModAwareId.ForDefaultMod("spawnIndicator")];
        var obj = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(
            blueprint, owner, Level.GetPosition(tile).WithZ(0.U()));

        var spawnIndicator = new SpawnIndicator(tile);
        obj.AddComponent(spawnIndicator);

        futureEnemySpawnIndicator = spawnIndicator;
        return obj;
    }

    public static GameObject CreateEnemyPathIndicator(Tile tile, NextDirectionFinder.Bias bias)
    {
        var obj = GameObjectFactory.CreateWithoutRenderer(null, Level.GetPosition(tile).WithZ(0.U()));
        obj.AddComponent(new EnemyPathIndicator(tile, bias));
        return obj;
    }
}
