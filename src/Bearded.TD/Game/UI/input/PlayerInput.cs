﻿using amulware.Graphics;
using Bearded.TD.Utilities.Input;

namespace Bearded.TD.Game.UI
{
    class PlayerInput
    {
        private readonly GameInstance game;
        private readonly InteractionHandler defaultInteractionHandler;
        private ICursorHandler cursor;
        private InteractionHandler interactionHandler;

        public PlayerInput(GameInstance game)
        {
            this.game = game;

            cursor = new MouseCursorHandler(game.Camera, game.State.Level);
            defaultInteractionHandler = new DefaultInteractionHandler(game);
            ResetInteractionHandler();
        }

        public void HandleInput(UpdateEventArgs args, InputState input)
        {
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
