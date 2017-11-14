using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Mods;
using Bearded.TD.Networking;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;
using Lidgren.Network;

namespace Bearded.TD.UI.Model.Loading
{
    abstract class LoadingManager
    {
        public GameInstance Game { get; }
        protected IDispatcher<GameInstance> Dispatcher { get; }
        public NetworkInterface Network { get; }
        protected Logger Logger { get; }
        private readonly List<ModForLoading> modsForLoading = new List<ModForLoading>();

        private bool hasModsQueuedForLoading => modsForLoading.Count > 0;
        private bool haveModsFinishedLoading;

        protected LoadingManager(
            GameInstance game, IDispatcher<GameInstance> dispatcher, NetworkInterface networkInterface, Logger logger)
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
            if (!hasModsQueuedForLoading)
            {
                DebugAssert.State.Satisfies(Game.Me.ConnectionState == PlayerConnectionState.LoadingMods);
                Game.ContentManager.Mods.ForEach(loadMod);
            }
            if (!haveModsFinishedLoading && modsForLoading.All(mod => mod.IsDone))
            {
                gatherModBlueprints();
            }
        }

        private void loadMod(ModMetadata modMetadata)
        {
            var modForLoading = modMetadata.PrepareForLoading();
            var context = new ModLoadingContext(Logger);
            modForLoading.StartLoading(context);
            modsForLoading.Add(modForLoading);
        }

        private void gatherModBlueprints()
        {
            Game.SetBlueprints(Blueprints.Merge(
                modsForLoading.Select(modForLoading => modForLoading.GetLoadedMod())
                    .Prepend(DebugMod.Create())
                    .Select(mod => mod.Blueprints)));

            Game.RequestDispatcher.Dispatch(
                ChangePlayerState.Request(Game.Me, PlayerConnectionState.AwaitingLoadingData));

            haveModsFinishedLoading = true;
        }

        public void IntegrateUI()
        {
            var camera = new GameCamera();

            Game.IntegrateUI(camera);
        }
    }
}
