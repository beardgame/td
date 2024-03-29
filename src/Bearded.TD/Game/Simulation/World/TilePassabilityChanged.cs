﻿using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.World;

readonly struct TilePassabilityChanged : IGlobalEvent
{
    public Tile Tile { get; }

    public TilePassabilityChanged(Tile tile)
    {
        Tile = tile;
    }
}