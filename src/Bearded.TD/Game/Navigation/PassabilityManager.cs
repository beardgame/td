using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Events;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Navigation
{
    sealed class PassabilityManager : IListener<TileTypeChanged>
    {
        private static readonly Dictionary<TileGeometry.TileType, Passabilities>
            passabilityByTileType = new Dictionary<TileGeometry.TileType, Passabilities>
            {
                { TileGeometry.TileType.Unknown, Passabilities.All },
                { TileGeometry.TileType.Floor, Passabilities.All },
                { TileGeometry.TileType.Wall, Passabilities.None },
                { TileGeometry.TileType.Crevice, ~Passabilities.WalkingUnit }
            };

        private readonly GameEvents events;

        private readonly Dictionary<Passability, PassabilityLayer> passabilityLayers;

        public PassabilityManager(GameEvents events, Level level)
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

        public PassabilityLayer GetLayer(Passability passability) => passabilityLayers[passability];
    }
}