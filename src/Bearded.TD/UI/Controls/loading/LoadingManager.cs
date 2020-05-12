using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Networking;
using Bearded.Utilities.IO;

namespace Bearded.TD.UI.Controls
{
    abstract class LoadingManager
    {
        public GameInstance Game { get; }
        public NetworkInterface Network { get; }
        protected IDispatcher<GameInstance> Dispatcher => Game.Meta.Dispatcher;
        protected Logger Logger => Game.Meta.Logger;

        protected LoadingManager(GameInstance game, NetworkInterface networkInterface)
        {
            Game = game;
            Network = networkInterface;
        }

        public virtual void Update(UpdateEventArgs args)
        {
            // Network handling.
            Network.ConsumeMessages();
            Game.UpdatePlayers(args);
        }

        public void PrepareUI()
        {
            Game.PrepareUI();
        }

        public void FinalizeUI() {}
    }
}
