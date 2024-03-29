using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Physics;

abstract class ObjectLayer
{
    private readonly MultiDictionary<Tile, GameObject> objectLookup = new();

    public IEnumerable<GameObject> GetObjectsOnTile(Tile tile) => objectLookup.Get(tile);

    public void AddObjectToTile(GameObject obj, Tile tile)
    {
        objectLookup.Add(tile, obj);
    }

    public void RemoveObjectFromTile(GameObject obj, Tile tile)
    {
        var removed = objectLookup.Remove(tile, obj);
        State.Satisfies(removed, "Attempted to remove object from a tile it was not on.");
    }
}
