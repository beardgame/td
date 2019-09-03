using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Input
{
    sealed class PlayerInput
    {
        private readonly InteractionHandler defaultInteractionHandler;
        private readonly ICursorHandler cursor;
        private InteractionHandler interactionHandler;

        public bool IsMouseFocused { get; private set; }
        public Position2 CursorPosition => cursor.CursorPosition;

        public PlayerInput(GameInstance game)
        {
            cursor = new MouseCursorHandler(game.Camera, game.CameraController, game.State.Level);
            defaultInteractionHandler = new DefaultInteractionHandler(game);
            ResetInteractionHandler();
        }

        public void Focus()
        {
            DebugAssert.State.Satisfies(!IsMouseFocused);
            IsMouseFocused = true;
            interactionHandler?.Start(cursor);
        }

        public void UnFocus()
        {
            DebugAssert.State.Satisfies(IsMouseFocused);
            IsMouseFocused = false;
            interactionHandler?.End(cursor);
        }

        public void HandleInput(InputState input)
        {
            if (IsMouseFocused)
            {
                cursor.HandleInput(input);
                interactionHandler.Update(cursor);
            }
        }

        public void SetInteractionHandler(InteractionHandler interactionHandler)
        {
            if (IsMouseFocused) this.interactionHandler?.End(cursor);
            this.interactionHandler = interactionHandler;
            if (IsMouseFocused) this.interactionHandler.Start(cursor);
        }

        public void ResetInteractionHandler() => SetInteractionHandler(defaultInteractionHandler);
    }
}
