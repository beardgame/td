using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using static Bearded.TD.Game.Simulation.Buildings.IBuildBuildingPrecondition;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("defaultTileOccupancy")]
class DefaultTileOccupancy : Component, IBuildBuildingPrecondition
{
    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IBuildingStateProvider, ITileOccupation>(Owner, Events,
            (_, _) => addToBuildingLayerIfNotGhost());
        Owner.Deleting += deleteFromBuildingLayerIfNotGhost;
    }

    private void addToBuildingLayerIfNotGhost()
    {
        if (isGhost())
            return;

        tryReplacingExistingBuildings();

        Owner.Game.BuildingLayer.AddBuilding(Owner);
    }

    private void tryReplacingExistingBuildings()
    {
        foreach (var tile in OccupiedTileAccumulator.AccumulateOccupiedTiles(Owner))
        {
            if (Owner.Game.BuildingLayer[tile] is not { } building)
                continue;

            var replacer = building.GetComponents<CanBeBuiltOn>().Single();

            replacer.Replace();
        }
    }

    private void deleteFromBuildingLayerIfNotGhost()
    {
        if (isGhost())
            return;

        Owner.Game.BuildingLayer.RemoveBuilding(Owner);
    }

    private bool isGhost()
    {
        return Owner.GetComponents<IBuildingStateProvider>().Single().State.IsGhost;
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public Result CanBuild(Parameters parameters)
    {
        var invalidTiles = parameters.Footprint.OccupiedTiles
            .Where(t => !isTileValidForBuilding(parameters.Game, t))
            .ToImmutableArray();

        var canBuild = invalidTiles.Length == 0;

        return canBuild
            ? Result.Valid
            : Result.InValid
                with
                {
                    BadTiles = invalidTiles,
                };
    }

    private static bool isTileValidForBuilding(GameState game, Tile tile)
    {
        return game.Level.IsValid(tile)
            && game.GeometryLayer[tile].Type == TileType.Floor
            && game.VisibilityLayer[tile].IsRevealed()
            && tileContainsNoBuildingOrReplaceableBuilding(game, tile);
    }

    private static bool tileContainsNoBuildingOrReplaceableBuilding(GameState game, Tile tile)
    {
        return game.BuildingLayer.GetOccupationFor(tile) == BuildingLayer.Occupation.None
            || game.BuildingLayer[tile]!.GetComponents<CanBeBuiltOn>().Any();
    }
}
