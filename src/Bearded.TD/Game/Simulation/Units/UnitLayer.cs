using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Units;

sealed class UnitLayer
{
    private readonly MultiDictionary<Tile, ComponentGameObject> enemyLookup = new();

    public IEnumerable<ComponentGameObject> GetUnitsOnTile(Tile tile) => enemyLookup.Get(tile);

    public void MoveEnemyBetweenTiles(Tile from, Tile to, ComponentGameObject unit)
    {
        RemoveEnemyFromTile(from, unit);
        AddEnemyToTile(to, unit);
    }

    public void AddEnemyToTile(Tile tile, ComponentGameObject unit)
    {
        enemyLookup.Add(tile, unit);
    }

    public void RemoveEnemyFromTile(Tile tile, ComponentGameObject unit)
    {
        var removed = enemyLookup.Remove(tile, unit);
        DebugAssert.State.Satisfies(removed, "Tried removing enemy from a tile it was not on.");
    }
}
