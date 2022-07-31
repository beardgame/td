using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Input;

sealed class DebugSpawnEnemyInteractionHandler : InteractionHandler
{
    private readonly ModAwareId enemyBlueprintName;

    public DebugSpawnEnemyInteractionHandler(GameInstance game, ModAwareId enemyBlueprintName)
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
                    Game.Blueprints.ComponentOwners[enemyBlueprintName],
                    Game.Ids.GetNext<GameObject>()));
        else if (cursor.Cancel.Hit)
            Game.PlayerInput.ResetInteractionHandler();
    }
}
