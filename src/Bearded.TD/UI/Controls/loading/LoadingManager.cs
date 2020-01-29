using System;
using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Meta;
using Bearded.TD.Networking;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.IO;

namespace Bearded.TD.UI.Controls
{
    abstract class LoadingManager
    {
        private enum Stage
        {
            Initial,
            LoadingMods,
            Paused,
            Finished
        }

        public GameInstance Game { get; }
        public NetworkInterface Network { get; }
        protected IDispatcher<GameInstance> Dispatcher => Game.Meta.Dispatcher;
        protected Logger Logger => Game.Meta.Logger;
        private readonly List<ModForLoading> modsForLoading = new List<ModForLoading>();

        private Stage stage = Stage.Initial;

        public IReadOnlyList<ModLoadingProfiler.BlueprintLoadingProfile> LoadedBlueprints { get; private set; } =
            new ModLoadingProfiler.BlueprintLoadingProfile[] { };
        public IReadOnlyList<string> LoadingBlueprints { get; private set; } = new string[] { };

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
            switch (stage)
            {
                case Stage.Initial:
                    DebugAssert.State.Satisfies(modsForLoading.Count == 0);
                    DebugAssert.State.Satisfies(Game.Me.ConnectionState == PlayerConnectionState.LoadingMods);
                    Game.ContentManager.Mods.ForEach(loadMod);
                    stage = Stage.LoadingMods;
                    break;
                case Stage.LoadingMods:
                    if (modsForLoading.All(mod => mod.IsDone))
                    {
                        onModsFinishedLoading();
                    }
                    break;
                case Stage.Paused:
                    break;
                case Stage.Finished:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void onModsFinishedLoading()
        {
            var hasErrors = modsForLoading.Any(m => !m.DidLoadSuccessfully);
            switch (UserSettings.Instance.Misc.ModLoadFinishBehavior)
            {
                case ModLoadFinishBehavior.DoNothing:
                    gatherModBlueprints();
                    break;
                case ModLoadFinishBehavior.ThrowOnError:
                    if (hasErrors)
                    {
                        modsForLoading.First(m => !m.DidLoadSuccessfully).Rethrow();
                    }
                    else
                    {
                        gatherModBlueprints();
                    }
                    break;
                case ModLoadFinishBehavior.PauseOnError:
                    if (hasErrors)
                    {
                        stage = Stage.Paused;
                    }
                    else
                    {
                        gatherModBlueprints();
                    }
                    break;
                case ModLoadFinishBehavior.AlwaysPause:
                    stage = Stage.Paused;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void loadMod(ModMetadata modMetadata)
        {
            var modForLoading = modMetadata.PrepareForLoading();
            var context = new ModLoadingContext(Logger, Game.ContentManager.GraphicsLoader);
            LoadedBlueprints = context.Profiler.LoadedBlueprints;
            LoadingBlueprints = context.Profiler.LoadingBlueprints;
            modForLoading.StartLoading(context);
            modsForLoading.Add(modForLoading);
        }

        private void gatherModBlueprints()
        {
            Game.SetBlueprints(Blueprints.Merge(
                modsForLoading
                    .Select(modForLoading => modForLoading.GetLoadedMod())
                    .Select(mod => mod.Blueprints)));

            Game.Request(ChangePlayerState.Request(Game.Me, PlayerConnectionState.AwaitingLoadingData));

            stage = Stage.Finished;
        }

        public void IntegrateUI()
        {
            Game.IntegrateUI();
        }

        public void Unpause()
        {
            if (stage != Stage.Paused) return;
            gatherModBlueprints();
        }
    }
}
