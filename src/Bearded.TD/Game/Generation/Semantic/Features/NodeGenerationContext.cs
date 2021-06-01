using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.Features
{
    sealed class NodeGenerationContext
    {
        private readonly LevelGenerationCommandAccumulator commandAccumulator;
        private readonly Tilemap<TileGeometry> tilemap;

        public IArea Tiles { get; }
        public ImmutableArray<Circle> Circles { get; }

        public Random Random { get; }

        // info about connections, etc.

        public NodeGenerationContext(Tilemap<TileGeometry> tilemap, IArea tiles,
            ImmutableArray<Circle> circles,
            LevelGenerationCommandAccumulator commandAccumulator, Random random)
        {
            this.tilemap = tilemap;
            Tiles = tiles;
            this.commandAccumulator = commandAccumulator;
            Circles = circles;
            Random = random;
        }

        public TileGeometry Get(Tile tile)
        {
            return tilemap[tile];
        }

        public void Set(Tile tile, TileGeometry geometry)
        {
            if (!Tiles.Contains(tile))
            {
                throw new ArgumentException("May not write to tile outside node.", nameof(tile));
            }

            tilemap[tile] = geometry;
        }

        public void PlaceSpawnLocation(Tile tile)
        {
            if (!Tiles.Contains(tile))
            {
                throw new ArgumentException("May not write to position outside node.", nameof(tile));
            }

            commandAccumulator.PlaceSpawnLocation(tile);
        }

        public void PlaceBuilding(IBuildingBlueprint blueprint, Tile rootTile, ExternalId<Faction> faction)
        {
            // TODO: we currently hardcode the 0 variant of the footprint group. Pending a footprint rework.
            var positionedFootprint = blueprint.FootprintGroup.Positioned(0, rootTile);
            if (!positionedFootprint.OccupiedTiles.All(tilemap.Contains))
            {
                throw new ArgumentException("May not place buildings outside node.", nameof(rootTile));
            }

            commandAccumulator.PlaceBuilding(blueprint, positionedFootprint, faction);
        }

        public void PlaceGameObject(IComponentOwnerBlueprint blueprint, Position3 position, Direction2 direction)
        {
            var tile = Level.GetTile(position);
            if (!Tiles.Contains(tile))
            {
                throw new ArgumentException("May not write to position outside node.", nameof(position));
            }

            commandAccumulator.PlaceGameObject(blueprint, position, direction);
        }
    }
}
