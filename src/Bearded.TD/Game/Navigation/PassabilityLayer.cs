using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Events;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using static Bearded.TD.Game.World.TileGeometry.TileType;

namespace Bearded.TD.Game.Navigation
{
    enum Passability : byte
    {
        WalkingUnit = 1,
        FlyingUnit = 2,
        Worker = 3,
        Projectile = 4
    }

    [Flags]
    enum Passabilities : byte
    {
        None = 0,

        WalkingUnit = 1 << (Passability.WalkingUnit - 1),
        FlyingUnit = 1 << (Passability.FlyingUnit - 1),
        Worker = 1 << (Passability.Worker - 1),
        Projectile = 1 << (Passability.Projectile - 1),

        All = 0xff
    }

    static class PassabilityExtensions
    {
        public static Passabilities AsFlags(this Passability passability)
            => (Passabilities) (1 << ((byte) passability - 1));
    }

    struct TilePassability
    {
        public bool IsPassable { get; }
        public Directions PassableDirections { get; }

        public TilePassability(bool isPassable, Directions passableDirections)
        {
            IsPassable = isPassable;
            PassableDirections = passableDirections;
        }

        public TilePassability WithPassability(bool isPassable)
        {
            return new TilePassability(isPassable, PassableDirections);
        }

        public TilePassability WithPassableDirections(Directions passableDirections)
        {
            return new TilePassability(IsPassable, passableDirections);
        }
    }

    class PassabilityLayer
    {
        private readonly Level level;
        private readonly Tilemap<TilePassability> tilemap;

        public PassabilityLayer(Level level)
        {
            this.level = level;
            tilemap = new Tilemap<TilePassability>(level.Radius);
        }

        // TODO: take buildings into account
        
        public bool HandleTilePassabilityChanged(Tile tile, bool isPassable)
        {
            var currentPassability = tilemap[tile];
            if (currentPassability.IsPassable == isPassable)
            {
                return false;
            }

            tilemap[tile] = currentPassability.WithPassability(isPassable);

            foreach (var dir in level.ValidDirectionsFrom(tile))
            {
                var neighbour = tile.Neighbour(dir);
                if (isPassable)
                    openDirection(neighbour, dir.Opposite());
                else
                    closeDirection(neighbour, dir.Opposite());
            }

            return true;
        }

        private void closeDirection(Tile tile, Direction direction)
        {
            var passability = tilemap[tile];
            var passableDirections = passability.PassableDirections.Except(direction);
            tilemap[tile] = passability.WithPassableDirections(passableDirections);
        }

        private void openDirection(Tile tile, Direction direction)
        {
            var passability = tilemap[tile];
            var passableDirections = passability.PassableDirections.And(direction);
            tilemap[tile] = passability.WithPassableDirections(passableDirections);
        }
    }

    sealed class PassabilityManager : IListener<TileTypeChanged>
    {
        private static readonly Dictionary<TileGeometry.TileType, Passabilities>
            passabilityByTileType = new Dictionary<TileGeometry.TileType, Passabilities>
            {
                { Unknown, Passabilities.All },
                { Floor, Passabilities.All },
                { Wall, Passabilities.None },
                { Crevice, ~Passabilities.WalkingUnit }
            };

        private readonly GameEvents events;

        private readonly Dictionary<Passability, PassabilityLayer> passabilityLayers;

        public PassabilityManager(Level level, GameEvents events)
        {
            this.events = events;
            
            events.Subscribe(this);

            passabilityLayers = ((Passability[]) Enum.GetValues(typeof(Passability)))
                .ToDictionary(p => p, _ => new PassabilityLayer(level));
        }

        public void HandleEvent(TileTypeChanged @event)
        {
            var (tile, type) = (@event.Tile, @event.Type);

            var hasChangedPassability = false;

            foreach (var (passability, layer) in passabilityLayers)
            {
                var isPassable = (passability.AsFlags() & passabilityByTileType[type]) != Passabilities.None;
                hasChangedPassability |= layer.HandleTilePassabilityChanged(tile, isPassable);
            }

            if (hasChangedPassability)
            {
                events.Send(new TilePassabilityChanged(tile));
            }
        }
    }
}
