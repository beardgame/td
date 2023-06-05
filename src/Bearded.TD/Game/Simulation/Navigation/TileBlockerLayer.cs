using System;
using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Navigation;

sealed class TileBlockerLayer
{
    private readonly GlobalGameEvents events;
    private readonly Dictionary<Tile, GameObject> objectLookup = new();

    public TileBlockerLayer(GlobalGameEvents events)
    {
        this.events = events;
    }

    public bool IsTileBlocked(Tile tile) => objectLookup.ContainsKey(tile);

    public void AddTileBlocker(GameObject obj, Tile tile)
    {
        if (objectLookup.ContainsKey(tile))
        {
            throw new InvalidOperationException("Cannot add multiple tile blockers to the same tile.");
        }
        objectLookup.Add(tile, obj);
        events.Send(new TileBlockerAdded(tile));
    }

    public void RemoveTileBlocker(GameObject obj, Tile tile)
    {
        if (!objectLookup.TryGetValue(tile, out var foundObj) || obj != foundObj)
        {
            throw new InvalidOperationException(
                $"Attempted to remove wrong tile blocker from tile. Found {foundObj} but expected {obj}.");
        }
        objectLookup.Remove(tile);
        events.Send(new TileBlockerRemoved(tile));
    }
}
