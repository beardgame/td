using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.UI
{
    class DebugSpawnEnemyClickHandler : IClickHandler
    {
        private readonly string enemyBlueprintName;

        public TileSelection Selection => TileSelection.FromFootprints(FootprintGroup.Single);

        public DebugSpawnEnemyClickHandler(string enemyBlueprintName)
        {
            this.enemyBlueprintName = enemyBlueprintName;
        }

        public void HandleHover(GameInstance game, PositionedFootprint footprint)
        { }

        public void HandleClick(GameInstance game, PositionedFootprint footprint)
        {
            footprint.OccupiedTiles
                .Where(t => t.IsValid && t.Info.IsPassable)
                .ForEach(tile => game.State.Meta.Dispatcher.RunOnlyOnServer(
                    SpawnUnit.Command,
                    game.State,
                    tile,
                    game.Blueprints.Units[enemyBlueprintName],
                    game.Ids.GetNext<GameUnit>()));
        }

        public void Enable(GameInstance game)
        { }

        public void Disable(GameInstance game)
        { }
    }
}
