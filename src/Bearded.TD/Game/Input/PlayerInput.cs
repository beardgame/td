using amulware.Graphics;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Input
{
    class PlayerInput
    {
        private readonly GameInstance game;
        private readonly InteractionHandler defaultInteractionHandler;
        private ICursorHandler cursor;
        private InteractionHandler interactionHandler;

        public bool IsMouseFocused { get; set; }
        public Position2 CursorPosition => cursor.CursorPosition;

        public PlayerInput(GameInstance game)
        {
            this.game = game;

            cursor = new MouseCursorHandler(game.Camera, game.State.Level);
            defaultInteractionHandler = new DefaultInteractionHandler(game);
            ResetInteractionHandler();
        }

        public void HandleInput(UpdateEventArgs args, InputState input)
        {
            // This is a hack until we get rid of the old UI stuff.
            if (!IsMouseFocused) input.Mouse.Capture();

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
