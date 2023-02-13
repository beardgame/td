using Bearded.Graphics;
using Bearded.TD.Game.Debug;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.UpdateLoop;
using Bearded.TD.Meta;
using Bearded.TD.Networking;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.Game;

sealed class GameRunner
{
    private readonly GameInstance game;
    private readonly NetworkInterface networkInterface;
    private readonly ITimeSource timeSource;

    private bool isGameStarted;
    private GameObject legs;

    public GameRunner(GameInstance game, NetworkInterface networkInterface, ITimeSource timeSource)
    {
        this.game = game;
        this.networkInterface = networkInterface;
        this.timeSource = timeSource;
        DebugGameManager.Instance.RegisterGame(game);

        initLegs();
    }

    public void HandleInput(InputState inputState)
    {
        game.PlayerInput.HandleInput(inputState);

        if (inputState.Keyboard.GetKeyState(Keys.R).Hit)
        {
            legs.Delete();
            initLegs();
        }
    }

    private void initLegs()
    {
        legs = new GameObject(null, Position3.Zero, Direction2.Zero);
        legs.AddComponent(new ProtoLegs(game));
        legs.AddComponent(new DefaultComponentRenderer());
        game.State.Add(legs);
    }

    public void Update(UpdateEventArgs args)
    {
        if (!isGameStarted)
        {
            isGameStarted = true;
        }

        game.CameraController.Update(args);
        game.PlayerCursors.Update(args);
        game.State.Meta.SoundScape.Update();
        game.State.Meta.SoundScape.SetListenerPosition(game.Camera.Position.WithZ(game.Camera.VisibleRadius));

        var elapsedTime = new TimeSpan(args.ElapsedTimeInS) * UserSettings.Instance.Debug.GameSpeed;
        if (elapsedTime > Constants.Game.MaxFrameTime)
            elapsedTime = Constants.Game.MaxFrameTime;

        

        networkInterface.ConsumeMessages();
        game.UpdatePlayers(args);

        if (game.State.Meta.GameOver) return;

        game.State.Meta.Events.Send(new FrameUpdateStarting());
        game.State.Navigator.Update();
        game.State.Advance(elapsedTime);

        game.State.Meta.Synchronizer.Synchronize(timeSource);
    }

    public void Shutdown()
    {
        networkInterface.Shutdown();
        game.State.Meta.SoundScape.Dispose();
        DebugGameManager.Instance.UnregisterGame();
    }
}
