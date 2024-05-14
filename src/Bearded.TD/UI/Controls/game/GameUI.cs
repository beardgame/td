using System.Diagnostics;
using Bearded.Graphics;
using Bearded.TD.Content;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Meta;
using Bearded.TD.Networking;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Input;
using Bearded.UI.Controls;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using Bearded.Utilities.Input;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.UI.Controls;

sealed class GameUI :
    UpdateableNavigationNode<GameUI.Parameters>,
    IListener<GameOverTriggered>,
    IListener<GameVictoryTriggered>,
    IListener<TechnologyTokenAwarded>,
    IListener<TechnologyTokenConsumed>
{
    private readonly UIUpdater uiUpdater = new();

    public GameInstance Game { get; private set; } = null!;
    public TimeSource TimeSource { get; private set; } = null!;
    private GameRunner runner = null!;
    private InputManager inputManager = null!;
    private FocusManager focusManager = null!;
    private ShortcutCapturer shortcutCapturer = null!;

    public GameUIController GameUIController { get; } = new();

    public ActionBar ActionBar { get; } = new();
    public CoreStatsUI CoreStats { get; } = new();
    public TechnologyWindow TechnologyUI { get; } = new();
    public StatisticsSideBar StatisticsSideBar { get; } = new();
    private Binding<bool> techTokenIsAvailable { get; } = new(false);
    public IReadonlyBinding<bool> TechTokenIsAvailable => techTokenIsAvailable;

    public event VoidEventHandler? FocusReset;
    public event VoidEventHandler? GameVictoryTriggered;
    public event VoidEventHandler? GameOverTriggered;
    public event VoidEventHandler? GameLeft;

    private GameDebugOverlay? debugOverlay;

    protected override void Initialize(DependencyResolver dependencies, Parameters parameters)
    {
        base.Initialize(dependencies, parameters);

        Game = parameters.Game;
        TimeSource = new TimeSource();
        runner = new GameRunner(parameters.Game, parameters.Network, TimeSource);

        inputManager = dependencies.Resolve<InputManager>();
        focusManager = dependencies.Resolve<FocusManager>();
        shortcutCapturer = dependencies.Resolve<ShortcutCapturer>();
        var content = dependencies.Resolve<ContentManager>();

        shortcutCapturer.AddLayer(GameUIController.Shortcuts);

        ActionBar.Initialize(Game, shortcutCapturer, content);
        CoreStats.Initialize(Game, shortcutCapturer);
        TechnologyUI.Initialize(Game, GameUIController.TechnologyModalVisibility, shortcutCapturer, content);
        StatisticsSideBar.Initialize(Game);

        Game.Meta.Events.Subscribe<GameOverTriggered>(this);
        Game.Meta.Events.Subscribe<TechnologyTokenAwarded>(this);
        Game.Meta.Events.Subscribe<TechnologyTokenConsumed>(this);

        FocusReset?.Invoke();
    }

    public override void Terminate()
    {
        ActionBar.Terminate();
        CoreStats.Terminate();
        TechnologyUI.Terminate();
        updateOverlayState(false, ref debugOverlay);

        shortcutCapturer.RemoveLayer(GameUIController.Shortcuts);

        base.Terminate();
    }

    public override void Update(UpdateEventArgs args)
    {
        if (focusManager.FocusedControl == null)
        {
            FocusReset?.Invoke();
        }

        var inputState = new InputState(inputManager);

        runner.HandleInput(inputState);
        runner.Update(args);

        TimeSource.Update(args);
        uiUpdater.Update(args);

        ActionBar.Update();
        CoreStats.Update();

        updateGameDebugOverlayState();
    }

    [Conditional("DEBUG")]
    private void updateGameDebugOverlayState()
    {
        updateOverlayState(UserSettings.Instance.Debug.GameDebugScreen, ref debugOverlay);
    }

    private void updateOverlayState<TOverlay>(bool showOverlay, ref TOverlay? overlay)
        where TOverlay : NavigationNode<Void>
    {
        switch (showOverlay)
        {
            case true when overlay == null:
            {
                overlay = Navigation!.Push<TOverlay>();
                break;
            }
            case false when overlay != null:
            {
                overlay.Terminate();
                Navigation!.Close(overlay);
                overlay = null;
                break;
            }
        }
    }

    public void SetWorldOverlay(IGameWorldOverlay overlay)
    {
        BuildingStatusObserver.Create(overlay, Game.SelectionManager);
    }

    public void HandleEvent(GameOverTriggered @event)
    {
        GameOverTriggered?.Invoke();
    }

    public void HandleEvent(GameVictoryTriggered @event)
    {
        GameVictoryTriggered?.Invoke();
    }

    public void HandleEvent(TechnologyTokenAwarded _)
    {
        techTokenIsAvailable.SetFromSource(true);
    }

    public void HandleEvent(TechnologyTokenConsumed _)
    {
        techTokenIsAvailable.SetFromSource(false);
    }

    public void OnReturnToMainMenuButtonClicked()
    {
        runner.Shutdown();
        Navigation!.Replace<MainMenu, Intent>(Intent.None, this);
        GameLeft?.Invoke();
    }

    public void OnResumeGameButtonClicked()
    {
        GameUIController.ResumeGame();
    }

    public record struct Parameters(GameInstance Game, NetworkInterface Network);
}
