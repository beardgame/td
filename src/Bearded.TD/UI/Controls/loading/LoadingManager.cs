using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Mods;
using Bearded.TD.Networking;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.IO;

namespace Bearded.TD.UI.Controls
{
    abstract class LoadingManager
    {
        public GameInstance Game { get; }
        public NetworkInterface Network { get; }
        protected IDispatcher<GameInstance> Dispatcher => Game.Meta.Dispatcher;
        protected Logger Logger => Game.Meta.Logger;
        private readonly List<ModForLoading> modsForLoading = new List<ModForLoading>();

        private bool hasModsQueuedForLoading => modsForLoading.Count > 0;
        private bool haveModsFinishedLoading;

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

            // Mod loading.
            if (!hasModsQueuedForLoading)
            {
                DebugAssert.State.Satisfies(Game.Me.ConnectionState == PlayerConnectionState.LoadingMods);
                Game.ContentManager.Mods.ForEach(loadMod);
            }
            
            if (!haveModsFinishedLoading && modsForLoading.All(mod => mod.IsDone))
            {
                gatherModBlueprints();
                foreach (var blueprint in Game.Blueprints.Buildings.All)
                {
                    Game.State.Technology.UnlockBlueprint(blueprint);
                }
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
