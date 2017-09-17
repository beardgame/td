using amulware.Graphics;
using Bearded.TD.Game.UI;
using Bearded.TD.Meta;
using Bearded.TD.Networking;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;
using Lidgren.Network;

namespace Bearded.TD.Game
{
    class GameRunner
    {
        private readonly GameInstance game;
        private readonly NetworkInterface networkInterface;
        private readonly InputManager inputManager;
        private readonly GameInputHandler inputHandler;

        public GameRunner(GameInstance game, NetworkInterface networkInterface, InputManager inputManager)
        {
            this.game = game;
            this.networkInterface = networkInterface;
            this.inputManager = inputManager;
            inputHandler = new GameInputHandler(game);
        }

        public void HandleInput(UpdateEventArgs args, InputState inputState)
        {
            inputHandler.HandleInput(args, GameInputContext.Construct(inputState, game.Camera));
            game.Camera.HandleInput(args.ElapsedTimeInSf);
        }

        public void Update(UpdateEventArgs args)
        {
            var elapsedTime = new TimeSpan(args.ElapsedTimeInS) * UserSettings.Instance.Debug.GameSpeed;

            game.Controller.Update(elapsedTime);

            foreach (var msg in networkInterface.GetMessages())
                if (msg.MessageType == NetIncomingMessageType.Data)
                    game.DataMessageHandler.HandleIncomingMessage(msg);

            if (game.State.Meta.GameOver) return;
            
            foreach (var f in game.State.Factions)
                if (f.HasResources)
                    f.Resources.DistributeResources(elapsedTime);
            game.State.Navigator.Update();
            game.State.Advance(elapsedTime);

            game.State.Meta.Synchronizer.Synchronize(game.State.Time);
        }
    }
}
