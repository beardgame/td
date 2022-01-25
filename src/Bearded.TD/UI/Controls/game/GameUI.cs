using System.Diagnostics;
using Bearded.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Meta;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Input;
using Bearded.UI.Controls;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using Bearded.Utilities.Input;

namespace Bearded.TD.UI.Controls;

sealed class GameUI :
    UpdateableNavigationNode<(GameInstance game, GameRunner runner)>,
    IListener<GameOverTriggered>,
    IListener<GameVictoryTriggered>
{
    private readonly UIUpdater uiUpdater = new();

    public GameInstance Game { get; private set; } = null!;
    public TimeSource TimeSource { get; private set; } = null!;
    private GameRunner runner = null!;
    private InputManager inputManager = null!;
    private FocusManager focusManager = null!;

    public GameNotificationsUI NotificationsUI { get; }
    public ActionBar ActionBar { get; }
    public GameStatusUI GameStatusUI { get; }
    public PlayerStatusUI PlayerStatusUI { get; }
    public TechnologyUI TechnologyUI { get; }

    public event VoidEventHandler? FocusReset;
    public event GenericEventHandler<IReportSubject>? EntityStatusOpened;
    public event VoidEventHandler? EntityStatusClosed;
    public event VoidEventHandler? GameVictoryTriggered;
    public event VoidEventHandler? GameOverTriggered;
    public event VoidEventHandler? GameLeft;

    public readonly Binding<bool> ShowDiegeticUI = new(true);

    private NavigationController? entityStatusNavigation;

    private GameDebugOverlay? debugOverlay;

    public GameUI()
    {
        NotificationsUI = new GameNotificationsUI();
        ActionBar = new ActionBar();
        GameStatusUI = new GameStatusUI();
        PlayerStatusUI = new PlayerStatusUI();
        TechnologyUI = new TechnologyUI();
    }

    protected override void Initialize(
        DependencyResolver dependencies, (GameInstance game, GameRunner runner) parameters)
    {
        base.Initialize(dependencies, parameters);

        Game = parameters.game;
        TimeSource = new TimeSource();
        runner = parameters.runner;

        inputManager = dependencies.Resolve<InputManager>();
        focusManager = dependencies.Resolve<FocusManager>();

        NotificationsUI.Initialize(Game, TimeSource);
        ActionBar.Initialize(Game);
        GameStatusUI.Initialize(Game);
        PlayerStatusUI.Initialize(Game);
        TechnologyUI.Initialize(Game);

        Game.SelectionManager.ObjectSelected += onObjectSelected;
        Game.SelectionManager.ObjectDeselected += onObjectDeselected;
        Game.Meta.Events.Subscribe<GameOverTriggered>(this);

        FocusReset?.Invoke();
    }

    public override void Terminate()
    {
        NotificationsUI.Terminate();
        ActionBar.Terminate();
        TechnologyUI.Terminate();
        updateOverlayState(false, ref debugOverlay);
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
        NotificationsUI.Update();
        GameStatusUI.Update();
        PlayerStatusUI.Update();
        TechnologyUI.Update();

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
                overlay = Navigation.Push<TOverlay>();
                break;
            }
            case false when overlay != null:
            {
                overlay.Terminate();
                Navigation.Close(overlay);
                overlay = null;
                break;
            }
        }
    }

    public void SetOverlayControl(IControlParent overlay)
    {
        DebugAssert.State.Satisfies(entityStatusNavigation == null, "Can only initialize entity status UI once.");

        var dependencies = new DependencyResolver();
        dependencies.Add(Game);
        dependencies.Add(uiUpdater);

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

    private void onObjectSelected(ISelectable selectedObject)
    {
        var subject = selectedObject.Subject;
        entityStatusNavigation!.ReplaceAll<ReportSubjectOverlay, IReportSubject>(subject);
        EntityStatusOpened?.Invoke(subject);
    }

    private void onObjectDeselected(ISelectable t)
    {
        entityStatusNavigation?.CloseAll();
        EntityStatusClosed?.Invoke();
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
        Navigation.Replace<MainMenu>(this);
        GameLeft?.Invoke();
    }
}
