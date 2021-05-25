using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Input
{
    sealed class PlayerInput
    {
        private readonly InteractionHandler defaultInteractionHandler;
        private readonly ICursorHandler cursor;

        private bool isMouseFocused;
        private InteractionHandler? interactionHandler;

        public Position2 CursorPosition => cursor.CursorPosition;

        public PlayerInput(GameInstance game)
        {
            cursor = new MouseCursorHandler(game.Camera, game.CameraController);
            defaultInteractionHandler = new DefaultInteractionHandler(game);
            ResetInteractionHandler();
        }

        public void Focus()
        {
            DebugAssert.State.Satisfies(!isMouseFocused);
            isMouseFocused = true;
            interactionHandler?.Start(cursor);
        }

        public void UnFocus()
        {
            DebugAssert.State.Satisfies(isMouseFocused);
            isMouseFocused = false;
            interactionHandler?.End(cursor);
        }

        public void HandleInput(InputState input)
        {
            if (isMouseFocused)
            {
                cursor.HandleInput(input);
                interactionHandler!.Update(cursor);
            }
        }

        public void SetInteractionHandler(InteractionHandler interactionHandler)
        {
            if (isMouseFocused)
            {
                this.interactionHandler?.End(cursor);
            }
            this.interactionHandler = interactionHandler;
            if (isMouseFocused)
            {
                this.interactionHandler.Start(cursor);
            }
        }

        public void ResetInteractionHandler() => SetInteractionHandler(defaultInteractionHandler);
    }
}
