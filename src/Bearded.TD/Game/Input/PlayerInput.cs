using Bearded.TD.Game.Meta;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.Game.Input;

sealed class PlayerInput
{
    private readonly GameInstance game;
    private readonly InteractionHandler defaultInteractionHandler;
    private readonly MouseCursorHandler cursor;
    private readonly UIDrawStateImpl uiDrawState = new();

    private bool isMouseFocused;
    private InteractionHandler? interactionHandler;

    public Position2 CursorPosition => cursor.CursorPosition;
    public IUIDrawState UIDrawState => uiDrawState;

    public PlayerInput(GameInstance game)
    {
        this.game = game;
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
        if (!isMouseFocused)
        {
            return;
        }

        cursor.HandleInput(input);
        interactionHandler!.Update(cursor);

        if (input.ForKey(Keys.Tab).Hit)
        {
            sendPing();
        }

        if (input.ForKey(Keys.X).Hit)
        {
            uiDrawState.StatusDisplayShown = !uiDrawState.StatusDisplayShown;
        }
        uiDrawState.InvertStatusDisplay = input.ForKey(Keys.LeftAlt).Active || input.ForKey(Keys.RightAlt).Active;
    }

    private void sendPing()
    {
        var request = Ping.Request(game, game.Me, cursor.CursorPosition);
        game.RequestDispatcher.Dispatch(game.Me, request);
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

    private class UIDrawStateImpl : IUIDrawState
    {
        public bool StatusDisplayShown { get; set; }
        public bool InvertStatusDisplay { get; set; }

        public bool DrawStatusDisplays => StatusDisplayShown ^ InvertStatusDisplay;
    }
}
