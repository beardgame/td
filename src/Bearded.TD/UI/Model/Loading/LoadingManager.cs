using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Mods;
using Bearded.TD.Networking;
using Bearded.Utilities.IO;
using Lidgren.Network;

namespace Bearded.TD.UI.Model.Loading
{
    abstract class LoadingManager
    {
        public GameInstance Game { get; }
        protected IDispatcher Dispatcher { get; }
        public NetworkInterface Network { get; }
        protected Logger Logger { get; }
        private readonly List<ModForLoading> modsForLoading = new List<ModForLoading>();

        protected bool HasModsQueuedForLoading => modsForLoading.Count > 0;
        protected bool HaveAllModsFinishedLoading => modsForLoading.All(mod => mod.IsLoaded);

        protected LoadingManager(
            GameInstance game, IDispatcher dispatcher, NetworkInterface networkInterface, Logger logger)
        {
            Game = game;
            Dispatcher = dispatcher;
            Network = networkInterface;
            Logger = logger;
        }

        public virtual void Update(UpdateEventArgs args)
        {
            foreach (var msg in Network.GetMessages())
                if (msg.MessageType == NetIncomingMessageType.Data)
                    Game.DataMessageHandler.HandleIncomingMessage(msg);
        }

        protected void LoadMod(ModMetadata modMetadata)
        {
            var modForLoading = modMetadata.PrepareForLoading();
            modForLoading.StartLoading();
            modsForLoading.Add(modForLoading);
        }

        public void IntegrateUI()
        {
            var camera = new GameCamera();

            Game.IntegrateUI(camera);
        }
    }
}
