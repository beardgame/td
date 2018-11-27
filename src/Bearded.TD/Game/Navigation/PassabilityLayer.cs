using System;
using System.Collections.Generic;
using Bearded.TD.Game.Events;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using static Bearded.TD.Game.Navigation.TilePassability.Passability;
using static Bearded.TD.Game.World.TileGeometry.TileType;

namespace Bearded.TD.Game.Navigation
{
    struct TilePassability
    {
        [Flags]
        public enum Passability : byte
        {
            None = 0,
            Building = 1 << 0,
            Unit = 1 << 1,
            Flying = 1 << 2,
            Worker = 1 << 3,
            Projectile = 1 << 4,

            All = 0xff,
        }
        
        // TODO: OMG improve the naming of all of this, and probably invert 'blocked'
        // TODO: should we store this value at all? we can just calculate it when needed?
        //     or maybe a separate layer, and then a bunch of 'open directions for X' layers?
        public Passability BlockedFor { get; }
        public Directions OpenDirectionsForUnits { get; private set; }

        public TilePassability(Passability blockedFor, Directions openDirectionsForUnits)
        {
            BlockedFor = blockedFor;
            OpenDirectionsForUnits = openDirectionsForUnits;
        }

        public TilePassability WithPassability(Passability blockedFor)
        {
            return new TilePassability(blockedFor, OpenDirectionsForUnits);
        }

        public TilePassability WithOpenDirectionsForUnits(Directions openDirectionsForUnits)
        {
            return new TilePassability(BlockedFor, openDirectionsForUnits);
        }

        public bool IsPassableFor(Passability layer)
        {
            return (BlockedFor & layer) == None;
        }
    }

    class PassabilityLayer : IListener<TileTypeChanged>
    {
        private static readonly Dictionary<TileGeometry.TileType, TilePassability.Passability>
            blockedLayersByType = new Dictionary<TileGeometry.TileType, TilePassability.Passability>
            {
                { Unknown, None },
                { Floor, None },
                { Wall, All },
                { Crevice, ~(Flying | Projectile) },
            };
        
        private readonly Level level;
        private readonly GameEvents events;
        private readonly Tilemap<TilePassability> tilemap;

        public PassabilityLayer(Level level, GameEvents events)
        {
            this.level = level;
            this.events = events;
            tilemap = new Tilemap<TilePassability>(level.Radius);
            
            events.Subscribe(this);
        }

        // TODO: need different open/closed values for walking, flying,
        //     burrowing (different events for different pathfinding trees?)
        // TODO: take buildings into account
        // TODO: stretch goal: only send events if it actually changed
        
        public void HandleEvent(TileTypeChanged @event)
        {
            var (tile, type) = (@event.Tile, @event.Type);
            var tileIsBlockedFor = blockedLayersByType[type];

            var tilePassability = tilemap[tile].WithPassability(tileIsBlockedFor);
            
            tilemap[tile] = tilePassability;

            var isPassable = tilePassability.IsPassableFor(Unit);

            foreach (var dir in level.ValidDirectionsFrom(tile))
            {
                var neighbour = tile.Neighbour(dir);
                if (isPassable)
                    openForUnitsTo(neighbour, dir.Opposite());
                else
                    closeForUnitsTo(neighbour, dir.Opposite());
            }

            events.Send(new TilePassabilityChanged(tile));
        }

        private void closeForUnitsTo(Tile tile, Direction direction)
        {
            var passability = tilemap[tile];
            var openDirections = passability.OpenDirectionsForUnits.Except(direction);
            tilemap[tile] = passability.WithOpenDirectionsForUnits(openDirections);
        }

        private void openForUnitsTo(Tile tile, Direction direction)
        {
            var passability = tilemap[tile];
            var openDirections = passability.OpenDirectionsForUnits.And(direction);
            tilemap[tile] = passability.WithOpenDirectionsForUnits(openDirections);
        }
    }
}
