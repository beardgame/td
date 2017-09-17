using amulware.Graphics;
using Bearded.TD.Game.UI;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game
{
    class GameInputHandler
    {
        private readonly GameInstance game;
        private ICursorHandler cursor;
        private InteractionHandler interactionHandler;
        private Position2 mousePosition;

        public GameInputHandler(GameInstance game)
        {
            this.game = game;
            game.State.Add(new Cursor(() => mousePosition));

            cursor = new MouseCursorHandler(game.State.Level);
            interactionHandler = new DefaultInteractionHandler(game);
            interactionHandler.Start(cursor);
        }

        public void HandleInput(UpdateEventArgs args, GameInputContext input)
        {
            mousePosition = input.MousePos;
            
            cursor.Update(args, input);
            interactionHandler.Update(args, input, cursor);
        }
    }
}
