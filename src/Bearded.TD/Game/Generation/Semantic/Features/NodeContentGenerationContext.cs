using System;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Generation.Semantic.Props;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.Features;

sealed class NodeContentGenerationContext
{
    private readonly LevelGenerationCommandAccumulator commandAccumulator;
    private readonly Accumulator<PropHint> propHintAccumulator;
    private readonly IArea tiles;

    public IArea BuildingTiles { get; private set; } = Area.Empty();

    public NodeContentGenerationContext(
        LevelGenerationCommandAccumulator commandAccumulator, Accumulator<PropHint> propHintAccumulator, IArea tiles)
    {
        this.commandAccumulator = commandAccumulator;
        this.propHintAccumulator = propHintAccumulator;
        this.tiles = tiles;
    }

    public void PlaceSpawnLocation(Tile tile)
    {
        if (!tiles.Contains(tile))
        {
            throw new ArgumentException("May not write to position outside node.", nameof(tile));
        }

        commandAccumulator.PlaceSpawnLocation(tile);
    }

    public void PlaceBuilding(IGameObjectBlueprint blueprint, Tile rootTile, ExternalId<Faction> faction)
    {
        var positionedFootprint = blueprint.GetFootprint().Positioned(rootTile, Orientation.Default);
        if (!positionedFootprint.OccupiedTiles.All(tiles.Contains))
        {
            throw new ArgumentException("May not place buildings outside node.", nameof(rootTile));
        }
        if (positionedFootprint.OccupiedTiles.Any(BuildingTiles.Contains))
        {
            throw new InvalidOperationException("May not place buildings on top of each other.");
        }

        BuildingTiles = Area.Union(BuildingTiles, Area.From(positionedFootprint.OccupiedTiles));
        commandAccumulator.PlaceBuilding(blueprint, positionedFootprint, faction);
    }

    public void PlaceGameObject(IGameObjectBlueprint blueprint, Position3 position, Direction2 direction)
    {
        var tile = Level.GetTile(position);
        if (!tiles.Contains(tile))
        {
            throw new ArgumentException("May not write to position outside node.", nameof(position));
        }

        commandAccumulator.PlaceGameObject(blueprint, position, direction);
    }

    public void PlacePropHint(Tile tile, PropPurpose purpose)
    {
        if (!tiles.Contains(tile))
        {
            throw new ArgumentException("May not write to position outside node.", nameof(tile));
        }

        propHintAccumulator.Contribute(new PropHint(tile, purpose));
    }
}
