using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Interaction
{
    class DebugSpawnEnemyClickHandler : IClickHandler
    {
        public Footprint Footprint => Footprint.Single;

        public void HandleHover(GameState game, Tile<TileInfo> rootTile)
        { }

        public void HandleClick(GameState game, Tile<TileInfo> rootTile)
        {
            if (rootTile.IsValid && rootTile.Info.IsPassable)
                game.Add(new EnemyUnit(new UnitBlueprint(100, new Speed(2)), rootTile));
        }

        public void Enable(GameState game)
        { }

        public void Disable(GameState game)
        { }
    }
}
