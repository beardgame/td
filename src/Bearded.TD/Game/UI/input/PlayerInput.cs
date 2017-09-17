using amulware.Graphics;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.UI
{
    class PlayerInput
    {
        private readonly GameInstance game;
        private readonly InteractionHandler defaultInteractionHandler;
        private ICursorHandler cursor;
        private InteractionHandler interactionHandler;
        private Position2 mousePosition;

        public PlayerInput(GameInstance game)
        {
            this.game = game;
            game.State.Add(new Cursor(() => mousePosition));

            cursor = new MouseCursorHandler(game.State.Level);
            defaultInteractionHandler = new DefaultInteractionHandler(game);
            ResetInteractionHandler();
        }

        public void HandleInput(UpdateEventArgs args, GameInputContext input)
        {
            mousePosition = input.MousePosition;

            cursor.Update(args, input);
            interactionHandler.Update(args, cursor);
        }

        public void SetInteractionHandler(InteractionHandler interactionHandler)
        {
            this.interactionHandler?.End(cursor);
            this.interactionHandler = interactionHandler;
            this.interactionHandler.Start(cursor);
        }

        public void ResetInteractionHandler() => SetInteractionHandler(defaultInteractionHandler);
    }
}
