using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.GameLoop;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.Linq;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.GameLoop;

sealed class SpawnLocation : Component, IIdable<SpawnLocation>, IListener<WaveEnded>, IDeletable
{
    private const double buildingPenalty = 1_000;

    private readonly Tile tile;
    private readonly HashSet<Id<Wave>> assignedWaves = new();
    private PassabilityLayer passability = null!;
    private VisibilityLayer visibility = null!;
    private BuildingLayer buildings = null!;
    private SpawnPlaceholder? placeholder;

    public Id<SpawnLocation> Id { get; }
    public Tile SpawnTile { get; private set; }
    public bool IsAwake { get; private set; }

    public bool Deleted => Owner?.Deleted ?? false;

    public SpawnLocation(Id<SpawnLocation> id, Tile tile)
    {
        Id = id;
        this.tile = tile;
        SpawnTile = tile;
    }

    protected override void OnAdded() {}

    public override void Activate()
    {
        base.Activate();
        Owner.Game.IdAs(Id, this);
        Owner.Game.ListAs(this);
        Owner.Game.Meta.Events.Subscribe(this);

        passability = Owner.Game.PassabilityObserver.GetLayer(Passability.Bulldozer);
        visibility = Owner.Game.VisibilityLayer;
        buildings = Owner.Game.BuildingLayer;
    }

    public override void OnRemoved()
    {
        Owner.Game.Meta.Events.Unsubscribe(this);
        Owner.Game.DeleteId(Id);
    }

    public void WakeUp()
    {
        IsAwake = true;
    }

    public void UpdateSpawnTile()
    {
        State.Satisfies(assignedWaves.Count == 0, "Cannot update the tile to spawn on while waves are assigned.");
        var enemyTargets = Owner.Game.Enumerate<EnemySink.ITarget>().ToImmutableArray();
        if (enemyTargets.IsEmpty)
        {
            Owner.Game.Meta.Logger.Debug?.Log(
                "Skipping moving the spawn tile to edge of visible area because no target found.");
            SpawnTile = tile;
            return;
        }

        var buildingTilesToIgnore = enemyTargets.SelectMany(target => target.AllOccupiedTiles).ToImmutableHashSet();
        var pathFinder = makePathFinder(buildingTilesToIgnore);
        var paths = enemyTargets
            .Select(target => pathFinder.FindPath(tile, target.Tile))
            .NotNull()
            .ToImmutableArray();

        if (Enumerable.MinBy(paths, path => path.Cost) is not { } shortestPath)
        {
            Owner.Game.Meta.Logger.Debug?.Log(
                "Skipping moving the spawn tile to edge of visible area because no shortest path to target found.");
            SpawnTile = tile;
            return;
        }

        if (findTileBeforeFirstRevealedTile(tile, shortestPath.Path) is not { } firstRevealedTile)
        {
            Owner.Game.Meta.Logger.Debug?.Log(
                "Skipping moving the spawn tile to edge of visible area because no tile on the path is revealed.");
            SpawnTile = tile;
            return;
        }

        SpawnTile = firstRevealedTile;
    }

    private Pathfinder makePathFinder(ImmutableHashSet<Tile> buildingTilesToIgnore)
    {
        return Pathfinder.WithTileCosts(tileCostFunction(buildingTilesToIgnore), 1);
    }

    private Pathfinder.TileCostFunction tileCostFunction(ImmutableHashSet<Tile> buildingTilesToIgnore)
    {
        return tileCost;

        double? tileCost(Tile toTile)
        {
            if (!passability[toTile].IsPassable)
            {
                return 1e6;
            }

            if (buildings.HasMaterializedBuilding(toTile) && !buildingTilesToIgnore.Contains(toTile))
            {
                return buildingPenalty;
            }

            return 1;
        }
    }

    private Tile? findTileBeforeFirstRevealedTile(Tile startTile, ImmutableArray<Direction> path)
    {
        var currentTile = startTile;
        foreach (var dir in path)
        {
            var nextTile = currentTile.Neighbor(dir);
            if (!visibility[currentTile].IsRevealed() && visibility[nextTile].IsRevealed())
            {
                return currentTile;
            }
            currentTile = nextTile;
        }

        return null;
    }

    public void AssignWave(Id<Wave> wave, IEnumerable<EnemyForm> enemies)
    {
        State.Satisfies(IsAwake);
        assignedWaves.Add(wave);

        if (placeholder == null)
        {
            createSpawnPlaceholder();
        }

        foreach (var group in enemies.GroupBy(form => form))
        {
            placeholder!.FutureEnemySpawns.AddFutureEnemySpawn(group.Key, group.Count());
        }
    }

    private void createSpawnPlaceholder()
    {
        State.Satisfies(placeholder == null);
        var placeholderObj = GameLoopObjectFactory.CreateSpawnIndicator(Owner, SpawnTile, out var futureEnemySpawns);
        Owner.Game.Add(placeholderObj);
        placeholder = new SpawnPlaceholder(placeholderObj, futureEnemySpawns);
    }

    public void OnEnemySpawned(EnemyForm form)
    {
        placeholder?.FutureEnemySpawns.FulfilFutureEnemySpawn(form);
    }

    public void HandleEvent(WaveEnded @event)
    {
        assignedWaves.Remove(@event.Wave.Id);

        if (assignedWaves.Count > 0)
        {
            return;
        }

        placeholder?.GameObject.Delete();
        placeholder = null;
    }

    public override void Update(TimeSpan elapsedTime) {}

    private sealed record SpawnPlaceholder(GameObject GameObject, IFutureEnemySpawnIndicator FutureEnemySpawns);

    public override string ToString()
    {
        return $"Spawn location {Id.Value} @{tile} (@{SpawnTile})";
    }
}
