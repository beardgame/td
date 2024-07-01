using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Buildings;

static class BuildableTileChecker
{
    public static bool TileIsBuildable(GameState game, Tile tile) =>
        tileIsFloor(game, tile) &&
        tileContainsNoNonBuildingTileBlocker(game, tile) &&
        tileContainsNoBuildingOrReplaceableBuilding(game, tile);

    private static bool tileIsFloor(GameState game, Tile tile)
    {
        return game.GeometryLayer[tile].Type == TileType.Floor;
    }

    private static bool tileContainsNoNonBuildingTileBlocker(GameState game, Tile tile)
    {
        return game.TileBlockerLayer[tile]?.GetComponents<IBuildingStateProvider>().Any() ?? true;
    }

    private static bool tileContainsNoBuildingOrReplaceableBuilding(GameState game, Tile tile)
    {
        return game.BuildingLayer.GetObjectsOnTile(tile).All(b => b.TryGetSingleComponent<CanBeBuiltOn>(out _));
    }
}
