using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Events;
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

        private readonly ModLoadingProfiler profiler = new ModLoadingProfiler();
        private readonly Dictionary<string, ModForLoading> modsByModId = new Dictionary<string, ModForLoading>();
        private readonly Queue<ModMetadata> modLoadingQueue = new Queue<ModMetadata>();

        private Stage stage = Stage.Initial;
        private int numBlueprintsWithErrors;

        public ImmutableList<ModLoadingProfiler.BlueprintLoadingProfile> LoadedBlueprints =>
            profiler?.LoadedBlueprints ?? ImmutableList<ModLoadingProfiler.BlueprintLoadingProfile>.Empty;

        public ImmutableList<string> LoadingBlueprints => profiler?.LoadingBlueprints ?? ImmutableList<string>.Empty;

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
                    DebugAssert.State.Satisfies(modsByModId.Count == 0);
                    DebugAssert.State.Satisfies(modLoadingQueue.Count == 0);
                    DebugAssert.State.Satisfies(Game.Me.ConnectionState == PlayerConnectionState.LoadingMods);
                    Game.ContentManager.Mods.ForEach(modLoadingQueue.Enqueue);
                    startLoadingFromQueue();
                    stage = Stage.LoadingMods;
                    break;
                case Stage.LoadingMods:
                    startLoadingFromQueue();
                    if (modLoadingQueue.Count == 0 && modsByModId.Values.All(mod => mod.IsDone))
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

        private void startLoadingFromQueue()
        {
            while (modLoadingQueue.Count > 0 &&
                   modLoadingQueue.Peek().Dependencies.All(dependency => isModDoneLoading(dependency.Id)))
            {
                loadMod(modLoadingQueue.Dequeue());
            }
        }

        private bool isModDoneLoading(string modId) => modsByModId.ContainsKey(modId) && modsByModId[modId].IsDone;

        private void loadMod(ModMetadata modMetadata)
        {
            var modForLoading = modMetadata.PrepareForLoading();
            var context = new ModLoadingContext(Logger, Game.ContentManager.GraphicsLoader, profiler);
            var loadedDependencies = modMetadata.Dependencies
                .Select(dependency => modsByModId[dependency.Id].GetLoadedMod()).ToList().AsReadOnly();
            modForLoading.StartLoading(context, loadedDependencies);
            modsByModId[modMetadata.Id] = modForLoading;
        }

        private void onModsFinishedLoading()
        {
            var hasModWithTerminalFailure = modsByModId.Values.Any(m => !m.DidLoadSuccessfully);
            numBlueprintsWithErrors =
                profiler.LoadedBlueprints.Count(b => b.LoadingResult == ModLoadingProfiler.LoadingResult.HasError);

            switch (UserSettings.Instance.Misc.ModLoadFinishBehavior)
            {
                case ModLoadFinishBehavior.DoNothing:
                    gatherModBlueprints();
                    break;
                case ModLoadFinishBehavior.ThrowOnError:
                    if (hasModWithTerminalFailure)
                    {
                        modsByModId.Values.First(m => !m.DidLoadSuccessfully).Rethrow();
                    }
                    else
                    {
                        gatherModBlueprints();
                    }

                    break;
                case ModLoadFinishBehavior.PauseOnError:
                    if (hasModWithTerminalFailure)
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

        private void gatherModBlueprints()
        {
            Game.SetBlueprints(Blueprints.Merge(
                modsByModId.Values
                    .Select(modForLoading => modForLoading.GetLoadedMod())
                    .Select(mod => mod.Blueprints)));

            Game.Request(ChangePlayerState.Request(Game.Me, PlayerConnectionState.AwaitingLoadingData));

            stage = Stage.Finished;
        }

        public void PrepareUI()
        {
            Game.PrepareUI();
        }

        public void FinalizeUI()
        {
            if (numBlueprintsWithErrors > 0)
            {
                Game.Meta.Events.Send(new ModFilesFailedLoading(numBlueprintsWithErrors));
            }
        }

        public void Unpause()
        {
            if (stage != Stage.Paused) return;
            gatherModBlueprints();
        }
    }
}
