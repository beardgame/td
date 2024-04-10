using System;
using System.Diagnostics;
using Bearded.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Meta;
using Bearded.TD.Networking;
using Bearded.TD.Shared.Events;
using Bearded.TD.UI.Tooltips;
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
    IListener<GameVictoryTriggered>
{
    private readonly UIUpdater uiUpdater = new();

    public GameInstance Game { get; private set; } = null!;
    public TimeSource TimeSource { get; private set; } = null!;
    public TooltipFactory Tooltips { get; private set; } = null!;
    private GameRunner runner = null!;
    private InputManager inputManager = null!;
    private FocusManager focusManager = null!;
    private ShortcutCapturer shortcutCapturer = null!;

    public GameUIController GameUIController { get; } = new();

    public GameNotificationsUI NotificationsUI { get; }
    public ActionBar ActionBar { get; } = new();
    public CoreStatsUI CoreStats { get; } = new();
    public TechnologyWindow TechnologyUI { get; } = new();
    public StatisticsSideBar StatisticsSideBar { get; } = new();

    public event VoidEventHandler? FocusReset;
    public event VoidEventHandler? GameVictoryTriggered;
    public event VoidEventHandler? GameOverTriggered;
    public event VoidEventHandler? GameLeft;

    private NavigationController? entityStatusNavigation;

    private GameDebugOverlay? debugOverlay;

    public GameUI()
    {
        NotificationsUI = new GameNotificationsUI(GameUIController);
    }

    protected override void Initialize(DependencyResolver dependencies, Parameters parameters)
    {
        base.Initialize(dependencies, parameters);

        Game = parameters.Game;
        TimeSource = new TimeSource();
        runner = new GameRunner(parameters.Game, parameters.Network, TimeSource);

        inputManager = dependencies.Resolve<InputManager>();
        focusManager = dependencies.Resolve<FocusManager>();
        shortcutCapturer = dependencies.Resolve<ShortcutCapturer>();
        Tooltips = dependencies.Resolve<TooltipFactory>();

        shortcutCapturer.AddLayer(GameUIController.Shortcuts);

        NotificationsUI.Initialize(Game, TimeSource);
        ActionBar.Initialize(Game, shortcutCapturer);
        CoreStats.Initialize(Game, shortcutCapturer);
        TechnologyUI.Initialize(Game, GameUIController.TechnologyModalVisibility, shortcutCapturer, Tooltips);
        StatisticsSideBar.Initialize(Game, Tooltips);

        Game.SelectionManager.ObjectSelected += onObjectSelected;
        Game.SelectionManager.ObjectDeselected += onObjectDeselected;
        Game.Meta.Events.Subscribe<GameOverTriggered>(this);

        FocusReset?.Invoke();
    }

    public override void Terminate()
    {
        NotificationsUI.Terminate();
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
        NotificationsUI.Update();

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

    [Obsolete("Will be replaced by building status")]
    public void SetOverlayControl(IControlParent overlay)
    {
        DebugAssert.State.Satisfies(entityStatusNavigation == null, "Can only initialize entity status UI once.");

        var dependencies = new DependencyResolver();
        dependencies.Add(Game);
        dependencies.Add(uiUpdater);
        dependencies.Add(shortcutCapturer);

        var (models, views) = NavigationFactories.ForBoth()
            .Add<ReportSubjectOverlay, IReportSubject>(m => new ReportSubjectOverlayControl(m))
            .ToDictionaries();

        entityStatusNavigation = new NavigationController(
            overlay,
            dependencies,
            models,
            views);
        entityStatusNavigation!.Exited += Game.SelectionManager.ResetSelection;
    }

    public void SetWorldOverlay(IGameWorldOverlay overlay)
    {
        BuildingStatusObserver.Create(overlay, Game.SelectionManager);
    }

    private void onObjectSelected(ISelectable selectedObject)
    {
        var subject = selectedObject.Subject;
        entityStatusNavigation!.ReplaceAll<ReportSubjectOverlay, IReportSubject>(subject);
        GameUIController.ShowEntityStatus(new GameUIController.OpenEntityStatus(Game.SelectionManager.ResetSelection));
    }

    private void onObjectDeselected(ISelectable t)
    {
        entityStatusNavigation?.CloseAll();
        GameUIController.HideEntityStatus();
    }

    public void HandleEvent(GameOverTriggered @event)
    {
        GameOverTriggered?.Invoke();
    }

    public void HandleEvent(GameVictoryTriggered @event)
    {
        GameVictoryTriggered?.Invoke();
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
