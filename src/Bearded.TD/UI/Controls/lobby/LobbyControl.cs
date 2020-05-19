using System;
using amulware.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.UI.Layers;
using Bearded.UI.Controls;
using Bearded.Utilities;
using OpenTK;
using static Bearded.TD.UI.Factories.LegacyDefault;

namespace Bearded.TD.UI.Controls
{
    sealed class LobbyControl : CompositeControl
    {
        public LobbyControl(Lobby model)
        {
            // main buttons
            Add(
                new CompositeControl // ButtonGroup
                {
                    Button("Toggle ready")
                        .Anchor(a => a.Top(margin: 0, height: 50))
                        .Subscribe(b => b.Clicked += model.OnToggleReadyButtonClicked)
                        // TODO: should be IsEnabled instead of IsVisible, but there's no rendering difference
                        .Subscribe(b => model.LoadingUpdated += () => b.IsVisible = model.CanToggleReady),
                    Button("Back to menu")
                        .Anchor(a => a.Top(margin: 50, height: 50))
                        .Subscribe(b => b.Clicked += model.OnBackToMenuButtonClicked),
                }.Anchor(a => a.Left(margin: 8, width: 250).Bottom(margin: 8, height: 100))
            );

            // game settings
            var levelSize =
                new NumericInput(model.LevelSize)
                    {
                        MinValue = 10,
                        MaxValue = 100,
                        IsEnabled = model.CanChangeGameSettings
                    }
                    .Anchor(a => a.Top(margin: 0, height: 32))
                    .Subscribe(b => b.ValueChanged += newValue =>
                    {
                        if (b.IsEnabled) model.OnSetLevelSize(newValue);
                    });
            Add(
                new CompositeControl // ButtonGroup
                {
                    levelSize,
                    Button(() => model.WorkerDistributionMethod.ToString())
                        .Anchor(a => a.Top(margin: 36, height: 32))
                        .Subscribe(b => b.Clicked += model.OnCycleWorkerDistributionMethod),
                    Button(() => model.LevelGenerationMethod.ToString())
                        .Anchor(a => a.Top(margin: 72, height: 32))
                        .Subscribe(b => b.Clicked += model.OnCycleLevelGenerationMethod)
                }.Anchor(a => a.Left(margin: 8, width: 250).Top(margin: 8, height: 136))
            );

            var playerList = new ListControl {ItemSource = new PlayerListItemSource(model)}
                .Anchor(a => a
                    .Left(margin: 4, relativePercentage: .5)
                    .Right(margin: 8)
                    .Top(margin: 8)
                    .Bottom(margin: 4, relativePercentage: .5));
            Add(playerList);

            var loadingList = new ListControl(new ViewportClippingLayerControl())
            {
                ItemSource = new LoadingBlueprintsListSource(model.LoadingProfiler),
                StickToBottom = true
            };
            Add(
                new CompositeControl
                {
                    new Border(),
                    loadingList.Anchor(a => a.Left(4).Right(4))
                }.Anchor(a => a
                    .Left(margin: 4, relativePercentage: .5)
                    .Right(margin: 8)
                    .Top(margin: 4, relativePercentage: .5)
                    .Bottom(margin: 8))
            );

            model.PlayersChanged += playerList.Reload;
            model.LoadingUpdated += loadingList.Reload;
            model.GameSettingsChanged += () => { levelSize.Value = model.LevelSize; };
        }

        private sealed class PlayerListItemSource : IListItemSource
        {
            private readonly Lobby lobby;

            public int ItemCount => lobby.Players.Count;

            public PlayerListItemSource(Lobby lobby)
            {
                this.lobby = lobby;
            }

            public double HeightOfItemAt(int index) => LobbyPlayerRowControl.Height;

            public Control CreateItemControlFor(int index) => new LobbyPlayerRowControl(lobby.Players[index]);

            public void DestroyItemControlAt(int index, Control control)
            {
            }
        }

        private sealed class LoadingBlueprintsListSource : IListItemSource
        {
            private readonly ModLoadingProfiler profiler;

            public int ItemCount => profiler.LoadedBlueprints.Count + profiler.LoadingBlueprints.Count;

            public LoadingBlueprintsListSource(ModLoadingProfiler profiler)
            {
                this.profiler = profiler;
            }

            public double HeightOfItemAt(int index) => 16;

            public Control CreateItemControlFor(int index)
            {
                return index <  profiler.LoadedBlueprints.Count
                    ? LoadingBlueprintsListRow.ForLoaded(profiler.LoadedBlueprints[index])
                    : LoadingBlueprintsListRow.ForCurrentlyLoading(
                        profiler.LoadingBlueprints[index - profiler.LoadedBlueprints.Count]);
            }

            public void DestroyItemControlAt(int index, Control control)
            {
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
