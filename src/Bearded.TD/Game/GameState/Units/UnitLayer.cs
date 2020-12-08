using System.Collections.Generic;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.GameState.Units
{
    sealed class UnitLayer
    {
        private readonly MultiDictionary<Tile, EnemyUnit> enemyLookup = new MultiDictionary<Tile, EnemyUnit>();

        public IEnumerable<EnemyUnit> GetUnitsOnTile(Tile tile) => enemyLookup.Get(tile);

        public void MoveEnemyBetweenTiles(Tile from, Tile to, EnemyUnit unit)
        {
            RemoveEnemyFromTile(from, unit);
            AddEnemyToTile(to, unit);
        }

        public void AddEnemyToTile(Tile tile, EnemyUnit unit)
        {
            enemyLookup.Add(tile, unit);
        }

        public void RemoveEnemyFromTile(Tile tile, EnemyUnit unit)
        {
            var removed = enemyLookup.Remove(tile, unit);
            DebugAssert.State.Satisfies(removed, "Tried removing enemy from a tile it was not on.");
        }
    }
}
