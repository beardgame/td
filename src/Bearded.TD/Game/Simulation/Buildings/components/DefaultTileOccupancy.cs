using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using static Bearded.TD.Game.Simulation.Buildings.IBuildBuildingPrecondition;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("defaultTileOccupancy")]
class DefaultTileOccupancy : Component, IBuildBuildingPrecondition, IListener<Materialized>
{
    protected override void OnAdded() { }

    public override void Activate()
    {
        if (isGhost())
            return;

        tryReplacingExistingPlaceholders();
        addToBuildingLayer();
        Events.Subscribe(this);
        Owner.Deleting += deleteFromBuildingLayer;
    }

    public void HandleEvent(Materialized @event)
    {
        tryReplacingExistingBuildings(_ => true);
    }

    private void addToBuildingLayer()
    {
        Owner.Game.BuildingLayer.AddBuilding(Owner);
    }

    private void tryReplacingExistingPlaceholders()
    {
        tryReplacingExistingBuildings(b => !b.GetComponents<IBuildingStateProvider>().Single().State.IsMaterialized);
    }

    private void tryReplacingExistingBuildings(Func<GameObject, bool> selector)
    {
        var replacers = new HashSet<CanBeBuiltOn>();

        foreach (var tile in OccupiedTileAccumulator.AccumulateOccupiedTiles(Owner))
        {
            var buildings = Owner.Game.BuildingLayer[tile]
                .Where(building => building != Owner && selector(building));

            var rs = buildings.Select(building => building.GetComponents<CanBeBuiltOn>().Single());

            foreach (var r in rs)
            {
                replacers.Add(r);
            }
        }

        replacers.ForEach(r => r.Replace());
    }

    private void deleteFromBuildingLayer()
    {
        Owner.Game.BuildingLayer.RemoveBuilding(Owner);
    }

    private bool isGhost()
    {
        return Owner.GetComponents<IBuildingStateProvider>().SingleOrDefault()?.State.IsGhost ?? false;
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
                with
                {
                    AdditionalCost = -totalRefundsForReplacing(parameters.Game, parameters.Footprint.OccupiedTiles)
                }
            : Result.Invalid
                with
                {
                    BadTiles = invalidTiles
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
        return game.BuildingLayer[tile].All(b => b.TryGetSingleComponent<CanBeBuiltOn>(out _));
    }

    private static ResourceAmount totalRefundsForReplacing(GameState game, IEnumerable<Tile> tiles)
    {
        var buildingsToReplace = tiles
            .SelectMany(t => game.BuildingLayer[t])
            .Distinct();
        return buildingsToReplace
            .Select(obj => obj.TotalResourcesInvested().GetValueOrDefault(ResourceAmount.Zero))
            .Aggregate(ResourceAmount.Zero, (l, r) => l + r);
    }
}
