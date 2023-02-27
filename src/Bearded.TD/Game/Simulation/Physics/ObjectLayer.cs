using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Physics;

sealed class ObjectLayer
{
    private readonly MultiDictionary<Tile, GameObject> objectLookup = new();

    public IEnumerable<GameObject> GetObjectsOnTile(Tile tile) => objectLookup.Get(tile);

    public void AddObjectToTiles(GameObject obj, IEnumerable<Tile> tiles)
    {
        foreach (var tile in tiles)
        {
            AddObjectToTile(obj, tile);
        }
    }

    public void AddObjectToTile(GameObject obj, Tile tile)
    {
        objectLookup.Add(tile, obj);
    }

    public void RemoveObjectFromTiles(GameObject obj, IEnumerable<Tile> tiles)
    {
        foreach (var tile in tiles)
        {
            RemoveObjectFromTile(obj, tile);
        }
    }

    public void RemoveObjectFromTile(GameObject obj, Tile tile)
    {
        var removed = objectLookup.Remove(tile, obj);
        State.Satisfies(removed, "Attempted to remove object from a tile it was not on.");
    }
}
