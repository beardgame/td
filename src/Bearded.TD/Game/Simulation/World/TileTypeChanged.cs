﻿using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.World;

readonly struct TileTypeChanged : IGlobalEvent
{
    public Tile Tile { get; }
    public TileType Type { get; }

    public TileTypeChanged(Tile tile, TileType type)
    {
        Tile = tile;
        Type = type;
    }
}