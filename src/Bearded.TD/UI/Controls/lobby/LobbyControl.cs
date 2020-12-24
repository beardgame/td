using System;
using System.Collections.Immutable;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Meta;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using Bearded.Utilities;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.UI.Controls
{
    sealed class LobbyControl : CompositeControl
    {
        public LobbyControl(Lobby model)
        {
            var lobbyDetailsControl = new LobbyDetailsControl(model);

            this.BuildLayout()
                .ForFullScreen()
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
            var logsContent = new LogsControl(model);
            var logsWithTabs = new CompositeControl();
            logsWithTabs
                .BuildLayout()
                .AddTabs(t => t
                    .AddButton("Chat", logsContent.ShowChatLog)
                    .AddButton("Loading", logsContent.ShowLoadingLog))
                .FillContent(logsContent);

            sidebar.BuildLayout().DockFractionalSizeToBottom(logsWithTabs, 0.5);
        }

        private sealed class LogsControl : CompositeControl
        {
            private readonly Lobby model;
            private bool chatLogShown;
            private readonly ListControl chatList;
            private readonly ListControl loadingList;
            private readonly Control chatLog;
            private readonly Control loadingLog;
            private readonly Binding<string> chatInputBinding = new Binding<string>();

            public LogsControl(Lobby model)
            {
                this.model = model;

                // TODO: this is still very hardcoded with numbers
                var chatItemSource = new ChatLogListSource(model.ChatLog);
                chatList = new ListControl(new ViewportClippingLayerControl())
                {
                    ItemSource = chatItemSource,
                    StickToBottom = true
                };
                var chatInput = FormControlFactories.TextInput(chatInputBinding);
                chatLog =
                    new CompositeControl
                    {
                        new Border(),
                        chatList.Anchor(a => a.Left(4).Right(4).Bottom(Constants.UI.Text.LineHeight)),
                        chatInput.Anchor(a => a.Bottom(height: Constants.UI.Text.LineHeight))
                    };
                chatLog.IsVisible = true;
                Add(chatLog);
                model.ChatMessagesUpdated += () => chatList.ItemSource = chatItemSource;

                var loadingItemSource = new LoadingBlueprintsListSource(model.LoadingProfiler);
                loadingList = new ListControl(new ViewportClippingLayerControl())
                {
                    ItemSource = loadingItemSource,
                    StickToBottom = true
                };
                loadingLog =
                    new CompositeControl
                    {
                        new Border(),
                        loadingList.Anchor(a => a.Left(4).Right(4))
                    };
                loadingLog.IsVisible = false;
                Add(loadingLog);
                model.LoadingUpdated += loadingItemSource.Reload;
                model.LoadingUpdated += () => loadingList.ItemSource = loadingItemSource;
            }

            public void ShowChatLog()
            {
                chatLog.IsVisible = true;
                loadingLog.IsVisible = false;
                chatList.Reload();
                chatList.ScrollToBottom();
                chatLogShown = true;
            }

            public void ShowLoadingLog()
            {
                chatLog.IsVisible = false;
                loadingLog.IsVisible = true;
                loadingList.Reload();
                loadingList.ScrollToBottom();
                chatLogShown = false;
            }

            public override void KeyHit(KeyEventArgs eventArgs)
            {
                base.KeyHit(eventArgs);

                if (eventArgs.Handled || !chatLogShown || string.IsNullOrEmpty(chatInputBinding.Value)) return;

                if (eventArgs.Key == Keys.Enter)
                {
                    model.OnSendChatMessage(chatInputBinding.Value);
                    chatInputBinding.SetFromSource("");
                    eventArgs.Handled = true;
                }
            }
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
                var modStatusBindings = model.AvailableMods.ToDictionary(
                    mod => mod,
                    mod => Binding.Create(model.IsModEnabled(mod), newValue => model.OnSetModEnabled(mod, newValue)));
                var modStatuses = modStatusBindings.Select(pair => (pair.Key.Name, pair.Value)).ToList();
                var levelSize = Binding.Create(model.LevelSize, model.OnSetLevelSize);
                var workerDistributionMethod =
                    Binding.Create(model.WorkerDistributionMethod, model.OnSetWorkerDistributionMethod);
                var levelGenerationMethod =
                    Binding.Create(model.LevelGenerationMethod, model.OnSetLevelGenerationMethod);

                this.BuildScrollableColumn()
                    .AddHeader("Enabled mods")
                    .AddCollectionEditor(modStatuses)
                    .AddHeader("Game settings")
                    .AddForm(builder => builder
                        .AddNumberSelectRow("Level size", 10, 100, levelSize)
                        .AddDropdownSelectRow(
                            "Worker distribution method",
                            new[] {WorkerDistributionMethod.Neutral, WorkerDistributionMethod.RoundRobin},
                            e => e.ToString(),
                            workerDistributionMethod)
                        .AddDropdownSelectRow(
                            "Level generation method",
                            new[]
                            {
                                LevelGenerationMethod.Perlin,
                                LevelGenerationMethod.Legacy,
                                LevelGenerationMethod.Empty
                            },
                            e => e.ToString(),
                            levelGenerationMethod));

                model.ModsChanged += () =>
                {
                    foreach (var (mod, binding) in modStatusBindings)
                    {
                        binding.SetFromSource(model.IsModEnabled(mod));
                    }
                };
                model.GameSettingsChanged += () =>
                {
                    levelSize.SetFromSource(model.LevelSize);
                    workerDistributionMethod.SetFromSource(model.WorkerDistributionMethod);
                    levelGenerationMethod.SetFromSource(model.LevelGenerationMethod);
                };
            }
        }

        private sealed class ChatLogListSource : IListItemSource
        {
            private readonly ChatLog chatLog;

            public int ItemCount => chatLog.Messages.Count;

            public ChatLogListSource(ChatLog chatLog)
            {
                this.chatLog = chatLog;
            }

            public double HeightOfItemAt(int index) => Constants.UI.Text.LineHeight;

            public Control CreateItemControlFor(int index) =>
                TextFactories.Label(chatLog.Messages[index].GetDisplayString(), Label.TextAnchorLeft);

            public void DestroyItemControlAt(int index, Control control) {}
        }

        private sealed class LoadingBlueprintsListSource : IListItemSource
        {
            private readonly ModLoadingProfiler profiler;

            private ImmutableArray<ModLoadingProfiler.BlueprintLoadingProfile> loadedBlueprints;
            private ImmutableArray<string> loadingBlueprints;

            public int ItemCount => loadedBlueprints.Length + loadingBlueprints.Length;

            public LoadingBlueprintsListSource(ModLoadingProfiler profiler)
            {
                this.profiler = profiler;

                loadedBlueprints = profiler.LoadedBlueprints;
                loadingBlueprints = profiler.LoadingBlueprints;
            }

            public double HeightOfItemAt(int index) => 16;

            public Control CreateItemControlFor(int index)
            {
                return index <  loadedBlueprints.Length
                    ? LoadingBlueprintsListRow.ForLoaded(loadedBlueprints[index])
                    : LoadingBlueprintsListRow.ForCurrentlyLoading(loadingBlueprints[index - loadedBlueprints.Length]);
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
