using System;
using System.Collections.Immutable;
using amulware.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Generation;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;
using OpenTK;

namespace Bearded.TD.UI.Controls
{
    sealed class LobbyControl : CompositeControl
    {
        public LobbyControl(Lobby model)
        {
            var lobbyDetailsControl = new LobbyDetailsControl(model);

            this.BuildLayout()
                .AddNavBar(b => b
                    .WithBackButton("Back to menu", model.OnBackToMenuButtonClicked)
                    .WithForwardButton("Toggle ready", model.OnToggleReadyButtonClicked))
                .AddMainSidebar(c => fillSidebar(c, model))
                .AddTabs(t => t
                    .AddButton("Game settings", lobbyDetailsControl.ShowGameSettings)
                    .AddButton("Player list", lobbyDetailsControl.ShowPlayerList))
                .FillContent(lobbyDetailsControl);

            lobbyDetailsControl.ShowGameSettings();
        }

        private static void fillSidebar(IControlParent sidebar, Lobby model)
        {
            var itemSource = new LoadingBlueprintsListSource(model.LoadingProfiler);
            var loadingList = new ListControl(new ViewportClippingLayerControl())
            {
                ItemSource = itemSource,
                StickToBottom = true
            };
            sidebar.Add(
                new CompositeControl
                {
                    new Border(),
                    loadingList.Anchor(a => a.Left(4).Right(4))
                }.Anchor(a => a.Top(margin: 4, relativePercentage: .5))
            );
            model.LoadingUpdated += itemSource.Reload;
            model.LoadingUpdated += loadingList.Reload;
        }

        private sealed class LobbyDetailsControl : CompositeControl
        {
            private readonly Control gameSettings;
            private readonly LobbyPlayerList.ItemSource playerListItemSource;
            private readonly ListControl playerList;

            public LobbyDetailsControl(Lobby model)
            {
                gameSettings = new GameSettingsControl(model);
                playerListItemSource = new LobbyPlayerList.ItemSource(model);
                playerList = new ListControl {ItemSource = playerListItemSource};
                model.PlayersChanged += playerList.Reload;
            }

            public void ShowGameSettings()
            {
                RemoveAllChildren();
                this.BuildLayout()
                    .AddStatusSidebar(() => playerList)
                    .FillContent(gameSettings);
                setPlayerListCompact(true);
            }

            public void ShowPlayerList()
            {
                RemoveAllChildren();
                this.BuildLayout()
                    .FillContent(playerList);
                setPlayerListCompact(false);
            }

            private void setPlayerListCompact(bool isCompact)
            {
                playerListItemSource.IsCompact = isCompact;
                // This looks silly, but it's the only way to force a reload next frame, rather than immediately.
                // Forcing a reload immediately throws an exception, because the control hierarchy is not fully built.
                playerList.ItemSource = playerListItemSource;
            }
        }

        private sealed class GameSettingsControl : CompositeControl
        {
            public GameSettingsControl(Lobby model)
            {
                var levelSize = Binding.Create(model.LevelSize, model.OnSetLevelSize);
                var workerDistributionMethod =
                    Binding.Create(model.WorkerDistributionMethod, model.OnSetWorkerDistributionMethod);
                var levelGenerationMethod =
                    Binding.Create(model.LevelGenerationMethod, model.OnSetLevelGenerationMethod);

                Add(FormFactories.Form(builder => builder
                    .AddNumberSelectRow("Level size", 10, 100, levelSize)
                    .AddDropdownSelectRow(
                        "Worker distribution method",
                        new [] { WorkerDistributionMethod.Neutral, WorkerDistributionMethod.RoundRobin},
                        e => e.ToString(),
                        workerDistributionMethod)
                    .AddDropdownSelectRow(
                        "Level generation method",
                        new []
                        {
                            LevelGenerationMethod.Perlin,
                            LevelGenerationMethod.Legacy,
                            LevelGenerationMethod.Empty
                        },
                        e => e.ToString(),
                        levelGenerationMethod)));

                model.GameSettingsChanged += () =>
                {
                    levelSize.Value = model.LevelSize;
                    workerDistributionMethod.Value = model.WorkerDistributionMethod;
                    levelGenerationMethod.Value = model.LevelGenerationMethod;
                };
            }
        }

        private sealed class LoadingBlueprintsListSource : IListItemSource
        {
            private readonly ModLoadingProfiler profiler;

            private ImmutableList<ModLoadingProfiler.BlueprintLoadingProfile> loadedBlueprints;
            private ImmutableList<string> loadingBlueprints;

            public int ItemCount => loadedBlueprints.Count + loadingBlueprints.Count;

            public LoadingBlueprintsListSource(ModLoadingProfiler profiler)
            {
                this.profiler = profiler;

                loadedBlueprints = profiler.LoadedBlueprints;
                loadingBlueprints = profiler.LoadingBlueprints;
            }

            public double HeightOfItemAt(int index) => 16;

            public Control CreateItemControlFor(int index)
            {
                return index <  loadedBlueprints.Count
                    ? LoadingBlueprintsListRow.ForLoaded(loadedBlueprints[index])
                    : LoadingBlueprintsListRow.ForCurrentlyLoading(loadingBlueprints[index - loadedBlueprints.Count]);
            }

            public void DestroyItemControlAt(int index, Control control) {}

            public void Reload()
            {
                loadedBlueprints = profiler.LoadedBlueprints;
                loadingBlueprints = profiler.LoadingBlueprints;
            }
        }

        private sealed class LoadingBlueprintsListRow : CompositeControl
        {
            private LoadingBlueprintsListRow(string path, Color color, Maybe<TimeSpan> loadingTime)
            {
                Add(new Label(path)
                {
                    Color = color, FontSize = 14, TextAnchor = new Vector2d(0, .5)
                }.Anchor(a => a.Right(margin: 100)));
                loadingTime.Select(time =>
                        new Label($"{time:s\\.fff}s")
                        {
                            Color = color, FontSize = 14, TextAnchor = new Vector2d(1, .5)
                        }.Anchor(a => a.Right(width: 100)))
                    .Match(Add);
            }

            public static LoadingBlueprintsListRow ForCurrentlyLoading(string path) =>
                new LoadingBlueprintsListRow(path, Color.LightBlue, Maybe.Nothing);

            public static LoadingBlueprintsListRow ForLoaded(ModLoadingProfiler.BlueprintLoadingProfile profile) =>
                new LoadingBlueprintsListRow(profile.Path, color(profile), Maybe.Just(profile.LoadingTime));

            private static Color color(ModLoadingProfiler.BlueprintLoadingProfile profile)
            {
                return profile.LoadingResult switch
                {
                    ModLoadingProfiler.LoadingResult.Success => Color.White,
                    ModLoadingProfiler.LoadingResult.HasWarning => Color.Orange,
                    ModLoadingProfiler.LoadingResult.HasError => Color.Red,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }
}
