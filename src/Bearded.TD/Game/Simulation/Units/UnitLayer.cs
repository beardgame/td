using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Units;

sealed class UnitLayer
{
    private readonly MultiDictionary<Tile, GameObject> enemyLookup = new();

    public IEnumerable<GameObject> GetUnitsOnTile(Tile tile) => enemyLookup.Get(tile);

    public void MoveEnemyBetweenTiles(Tile from, Tile to, GameObject unit)
    {
        RemoveEnemyFromTile(from, unit);
        AddEnemyToTile(to, unit);
    }

    public void AddEnemyToTile(Tile tile, GameObject unit)
    {
        enemyLookup.Add(tile, unit);
    }

    public void RemoveEnemyFromTile(Tile tile, GameObject unit)
    {
        var removed = enemyLookup.Remove(tile, unit);
        DebugAssert.State.Satisfies(removed, "Tried removing enemy from a tile it was not on.");
    }
}
