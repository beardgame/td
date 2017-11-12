using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Mods;
using Bearded.TD.Networking;
using Bearded.TD.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;
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
        private bool haveModsFinishedLoading;

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
            // Network handling.
            foreach (var msg in Network.GetMessages())
            {
                if (msg.MessageType == NetIncomingMessageType.Data)
                {
                    Game.DataMessageHandler.HandleIncomingMessage(msg);
                }
            }

            // Mod loading.
            if (!HasModsQueuedForLoading)
            {
                DebugAssert.State.Satisfies(Game.Me.ConnectionState == PlayerConnectionState.LoadingMods);
                Game.ContentManager.Mods.ForEach(LoadMod);
            }
            if (!haveModsFinishedLoading && modsForLoading.All(mod => mod.IsLoaded))
            {
                gatherModBlueprints();
            }
        }

        private void gatherModBlueprints()
        {
            var blueprints = Game.Blueprints;

            foreach (var mod in modsForLoading.Select(modForLoading => modForLoading.GetLoadedMod()).Prepend(DebugMod.Create()))
            {
                mod.Units.All.ForEach(unit => blueprints.Units.Add(unit));
            }

            Game.RequestDispatcher.Dispatch(
                ChangePlayerState.Request(Game.Me, PlayerConnectionState.AwaitingLoadingData));
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
