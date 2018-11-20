﻿using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Events
{
    struct TileDrawInfoChanged : IEvent
    {
        public Tile Tile { get; }

        public TileDrawInfoChanged(Tile tile) => Tile = tile;
    }
}
