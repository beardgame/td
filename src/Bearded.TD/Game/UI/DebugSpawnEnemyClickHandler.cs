using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Utilities;

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
                .ForEach(tile => game.State.Meta.Dispatcher.RunOnlyOnServer(
                    SpawnEnemy.Command, game.State, tile, game.Blueprints.Units["debug"]));
        }

        public void Enable(GameInstance game)
        { }

        public void Disable(GameInstance game)
        { }
    }
}
