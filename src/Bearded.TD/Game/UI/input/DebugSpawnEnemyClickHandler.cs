using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Units;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.UI
{
    class DebugSpawnEnemyInteractionHandler : InteractionHandler
    {
        private readonly string enemyBlueprintName;

        public DebugSpawnEnemyInteractionHandler(GameInstance game, string enemyBlueprintName)
            : base(game)
        {
            this.enemyBlueprintName = enemyBlueprintName;
        }

        public override void Update(UpdateEventArgs args, ICursorHandler cursor)
        {
            if (cursor.ClickAction.Hit)
                cursor.CurrentFootprint.OccupiedTiles
                    .Where(t => t.IsValid && t.Info.IsPassable)
                    .ForEach(tile => Game.State.Meta.Dispatcher.RunOnlyOnServer(
                        SpawnUnit.Command,
                        Game.State,
                        tile,
                        Game.Blueprints.Units[enemyBlueprintName],
                        Game.Ids.GetNext<GameUnit>()));
        }
    }
}
