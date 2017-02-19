using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Interaction;
using Bearded.TD.Game.Tiles;
using Bearded.Utilities.Input;
using OpenTK.Input;

namespace Bearded.TD.Game
{
    class GameController
    {
        private static readonly IClickHandler[] clickHandlers = {
            new BuildingClickHandler(new BuildingBlueprint(TileSelection.Single, 100)), // 1
            new BuildingClickHandler(new BuildingBlueprint(TileSelection.Triangle, 300)), // 2
            null, // 3
            null, // 4
            null, // 5
            null, // 6
            null, // 7
            new DebugToggleTileTypeClickHandler(), // 8
            new DebugSpawnEnemyClickHandler(), // 9
            null // 0
        };

        private static readonly Key[] clickHandlerKeys =
        {
            Key.Number1, Key.Number2, Key.Number3, Key.Number4, Key.Number5,
            Key.Number6, Key.Number7, Key.Number8, Key.Number9, Key.Number0
        };

        private readonly GameInstance game;

        public GameController(GameInstance game)
        {
            this.game = game;
        }

        public void Update(PlayerInput input)
        {
            for (var i = 0; i < clickHandlers.Length; i++)
            {
                if (!InputManager.IsKeyHit(clickHandlerKeys[i])) continue;
                if (i == game.SelectedClickHandler)
                {
                    clickHandlers[i].Disable(game.State);
                    game.SelectedClickHandler = -1;
                    game.State.Meta.Logger.Debug.Log("Disabled click handler.");
                }
                else
                {
                    if (game.SelectedClickHandler >= 0)
                        clickHandlers[game.SelectedClickHandler].Disable(game.State);
                    clickHandlers[i].Enable(game.State);
                    game.SelectedClickHandler = i;
                    game.State.Meta.Logger.Debug.Log("Enabled click handler {0}.", i + 1);
                }
                break;
            }

            if (game.SelectedClickHandler >= 0)
            {
                var clickHandler = clickHandlers[game.SelectedClickHandler];
                var footprint = clickHandler.Selection.GetPositionedFootprint(game.State.Level, input.MousePos);
                clickHandler.HandleHover(game.State, footprint);
                if (input.ClickAction.Hit)
                {
                    clickHandler.HandleClick(game.State, footprint);
                }
            }
        }
    }
}
