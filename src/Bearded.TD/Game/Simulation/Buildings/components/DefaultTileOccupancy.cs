using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using static Bearded.TD.Game.Simulation.Buildings.IBuildBuildingPrecondition;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

// TODO: this class does too much, and should be split further:
// - The actual implementation of IBuildBuildingPrecondition is completely isolated from the rest of this class, and can
//   therefore be moved.
// - Tracking in the building layer (which is somewhat coupled with adding the TileBlocker component on materialize) is
//   somewhat separate
// - Doing the actual replacing on materialize is somewhat related to the build precondition (since now we actually need
//   to execute on what we promised), but there may be a more elegant solution to that.
[Component("defaultTileOccupancy")]
sealed class DefaultTileOccupancy : Component,
    IBuildBuildingPrecondition,
    IListener<Materialized>,
    IListener<ObjectDeleting>
{
    private ITilePresenceListener? buildingLayerPresence;

    protected override void OnAdded() { }

    public override void Activate()
    {
        if (isGhost())
            return;

        tryReplacingExistingPlaceholders();
        buildingLayerPresence = Owner.TrackTilePresenceInLayer(Owner.Game.BuildingLayer);
        Events.Subscribe<Materialized>(this);
        Events.Subscribe<ObjectDeleting>(this);
    }

    public void HandleEvent(Materialized @event)
    {
        tryReplacingExistingBuildings(_ => true);
        Owner.AddComponent(new TileBlocker());
    }

    public void HandleEvent(ObjectDeleting @event)
    {
        buildingLayerPresence?.Detach();
        buildingLayerPresence = null;
    }

    private void tryReplacingExistingPlaceholders()
    {
        tryReplacingExistingBuildings(b => !b.GetComponents<IBuildingStateProvider>().Single().State.IsMaterialized);
    }

    private void tryReplacingExistingBuildings(Func<GameObject, bool> selector)
    {
        var replacers = new HashSet<CanBeBuiltOn>();

        foreach (var tile in Owner.GetTilePresence().OccupiedTiles)
        {
            var buildings = Owner.Game.BuildingLayer.GetObjectsOnTile(tile)
                .Where(building => building != Owner && selector(building));

            var rs = buildings.Select(building => building.GetComponents<CanBeBuiltOn>().Single());

            foreach (var r in rs)
            {
                replacers.Add(r);
            }
        }

        replacers.ForEach(r => r.Replace());
    }

    private bool isGhost() => Owner.GetComponents<IBuildingStateProvider>().SingleOrDefault()?.State.IsGhost ?? false;

    public override void Update(TimeSpan elapsedTime) { }

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
                    Cost = -totalRefundsForReplacing(parameters.Game, parameters.Footprint.OccupiedTiles)
                }
            : Result.Invalid
                with
                {
                    BadTiles = invalidTiles
                };
    }

    private static bool isTileValidForBuilding(GameState game, Tile tile)
    {
        return
            game.Level.IsValid(tile) &&
            game.VisibilityLayer[tile].IsRevealed() &&
            BuildableTileChecker.TileIsBuildable(game, tile);
    }

    private static Resource<Scrap> totalRefundsForReplacing(GameState game, IEnumerable<Tile> tiles)
    {
        var buildingsToReplace = tiles
            .SelectMany(t => game.BuildingLayer.GetObjectsOnTile(t))
            .Distinct();
        return buildingsToReplace
            .Select(obj => obj.TotalResourcesInvested().GetValueOrDefault(Resource<Scrap>.Zero))
            .Aggregate(Resource<Scrap>.Zero, (l, r) => l + r);
    }
}
