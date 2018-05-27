using amulware.Graphics;
using Bearded.TD.Meta;
using Bearded.TD.Networking;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game
{
    class GameRunner
    {
        private readonly GameInstance game;
        private readonly NetworkInterface networkInterface;

        public GameRunner(GameInstance game, NetworkInterface networkInterface)
        {
            this.game = game;
            this.networkInterface = networkInterface;
        }

        public void HandleInput(UpdateEventArgs args, InputState inputState)
        {
            game.PlayerInput.HandleInput(args, inputState);
        }

        public void Update(UpdateEventArgs args)
        {
            var elapsedTime = new TimeSpan(args.ElapsedTimeInS) * UserSettings.Instance.Debug.GameSpeed;

            game.Controller.Update(elapsedTime);

            networkInterface.ConsumeMessages();
            game.UpdatePlayers(args);

            if (game.State.Meta.GameOver) return;
            
            foreach (var f in game.State.Factions)
                if (f.HasResources)
                    f.Resources.DistributeResources(elapsedTime);
            game.State.Navigator.Update();
            game.State.Advance(elapsedTime);

            game.State.Meta.Synchronizer.Synchronize(game);
        }
    }
}
