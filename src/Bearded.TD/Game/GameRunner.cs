using Bearded.Graphics;
using Bearded.TD.Game.Debug;
using Bearded.TD.Meta;
using Bearded.TD.Networking;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game
{
    sealed class GameRunner
    {
        private readonly GameInstance game;
        private readonly NetworkInterface networkInterface;

        private bool isGameStarted;

        public GameRunner(GameInstance game, NetworkInterface networkInterface)
        {
            this.game = game;
            this.networkInterface = networkInterface;
            DebugGameManager.Instance.RegisterGame(game);
        }

        public void HandleInput(InputState inputState)
        {
            game.PlayerInput.HandleInput(inputState);
            game.PlayerCursors.Update();
        }

        public void Update(UpdateEventArgs args)
        {
            if (!isGameStarted)
            {
                isGameStarted = true;
            }

            game.CameraController.Update(args);

            var elapsedTime = new TimeSpan(args.ElapsedTimeInS) * UserSettings.Instance.Debug.GameSpeed;

            networkInterface.ConsumeMessages();
            game.UpdatePlayers(args);

            if (game.State.Meta.GameOver) return;

            foreach (var f in game.State.Factions)
                if (f.HasResources)
                    f.Resources.DistributeResources();
            game.State.Navigator.Update();
            game.State.Advance(elapsedTime);

            game.State.Meta.Synchronizer.Synchronize(game);
        }

        public void Shutdown()
        {
            networkInterface.Shutdown();
            DebugGameManager.Instance.UnregisterGame();
        }
    }
}
