using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Navigation;
using Bearded.TD.Game.Units;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Input
{
    sealed class DebugSpawnEnemyInteractionHandler : InteractionHandler
    {
        private readonly string enemyBlueprintName;

        public DebugSpawnEnemyInteractionHandler(GameInstance game, string enemyBlueprintName)
            : base(game)
        {
            this.enemyBlueprintName = enemyBlueprintName;
        }

        public override void Update(ICursorHandler cursor)
        {
            if (cursor.Click.Hit)
                cursor.CurrentFootprint.OccupiedTiles
                    .Where(t => Game.State.Level.IsValid(t)
                        && Game.State.PassabilityManager.GetLayer(Passability.WalkingUnit)[t].IsPassable)
                    .ForEach(tile => Game.State.Meta.Dispatcher.RunOnlyOnServer(
                        SpawnUnit.Command,
                        Game.State,
                        tile,
                        Game.Blueprints.Units[enemyBlueprintName],
                        Game.Ids.GetNext<EnemyUnit>()));
            else if (cursor.Cancel.Hit)
                Game.PlayerInput.ResetInteractionHandler();
        }
    }
}
