using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Units;

sealed class UnitLayer
{
    private readonly MultiDictionary<Tile, GameObject> enemyLookup = new();

    public IEnumerable<GameObject> GetUnitsOnTile(Tile tile) => enemyLookup.Get(tile);
}
