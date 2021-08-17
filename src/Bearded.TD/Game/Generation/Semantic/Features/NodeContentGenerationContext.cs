using System;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Tiles;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.Features
{
    sealed class NodeContentGenerationContext
    {
        private readonly LevelGenerationCommandAccumulator commandAccumulator;
        private readonly IArea tiles;

        public NodeContentGenerationContext(LevelGenerationCommandAccumulator commandAccumulator, IArea tiles)
        {
            this.commandAccumulator = commandAccumulator;
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

        public void PlaceBuilding(IBuildingBlueprint blueprint, Tile rootTile, ExternalId<Faction> faction)
        {
            // TODO: we currently hardcode the 0 variant of the footprint group. Pending a footprint rework.
            var positionedFootprint = blueprint.GetFootprintGroup().Positioned(0, rootTile);
            if (!positionedFootprint.OccupiedTiles.All(tiles.Contains))
            {
                throw new ArgumentException("May not place buildings outside node.", nameof(rootTile));
            }

            commandAccumulator.PlaceBuilding(blueprint, positionedFootprint, faction);
        }

        public void PlaceGameObject(IComponentOwnerBlueprint blueprint, Position3 position, Direction2 direction)
        {
            var tile = Level.GetTile(position);
            if (!tiles.Contains(tile))
            {
                throw new ArgumentException("May not write to position outside node.", nameof(position));
            }

            commandAccumulator.PlaceGameObject(blueprint, position, direction);
        }
    }
}
