using System.Linq;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.UI
{
    class DebugSpawnEnemyClickHandler : IClickHandler
    {
        public TileSelection Selection => TileSelection.FromFootprints(FootprintGroup.Single);

        public void HandleHover(GameInstance game, PositionedFootprint footprint)
        { }

        public void HandleClick(GameInstance game, PositionedFootprint footprint)
        {
            footprint.OccupiedTiles
                .Where(t => t.IsValid && t.Info.IsPassable)
                .ForEach(tile => game.State.Add(new EnemyUnit(new UnitBlueprint(100, 25, new Speed(2), 10), tile)));
        }

        public void Enable(GameInstance game)
        { }

        public void Disable(GameInstance game)
        { }
    }
}
