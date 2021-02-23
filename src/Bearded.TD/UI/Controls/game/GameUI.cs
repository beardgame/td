using System.Diagnostics;
using Bearded.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Meta;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Input;
using Bearded.UI.Controls;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using Bearded.Utilities.Input;

namespace Bearded.TD.UI.Controls
{
    sealed class GameUI : UpdateableNavigationNode<(GameInstance game, GameRunner runner)>,
        IListener<GameOverTriggered>, IListener<GameVictoryTriggered>, IListener<BuildingConstructionStarted>
    {
        private readonly UIUpdater uiUpdater = new();

        public GameInstance Game { get; private set; } = null!;
        private GameRunner runner = null!;
        private InputManager inputManager = null!;
        private FocusManager focusManager = null!;

        public GameNotificationsUI NotificationsUI { get; }
        public ActionBar ActionBar { get; }
        public GameStatusUI GameStatusUI { get; }
        public PlayerStatusUI PlayerStatusUI { get; }
        public TechnologyUI TechnologyUI { get; }

        public event VoidEventHandler? FocusReset;
        public event GenericEventHandler<ISelectable>? EntityStatusOpened;
        public event VoidEventHandler? EntityStatusClosed;
        public event VoidEventHandler? GameVictoryTriggered;
        public event VoidEventHandler? GameOverTriggered;
        public event VoidEventHandler? GameLeft;

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
            runner = parameters.runner;

            inputManager = dependencies.Resolve<InputManager>();
            focusManager = dependencies.Resolve<FocusManager>();

            NotificationsUI.Initialize(Game);
            ActionBar.Initialize(Game);
            GameStatusUI.Initialize(Game);
            PlayerStatusUI.Initialize(Game);
            TechnologyUI.Initialize(Game);

            Game.SelectionManager.ObjectSelected += onObjectSelected;
            Game.SelectionManager.ObjectDeselected += onObjectDeselected;
            Game.Meta.Events.Subscribe<GameOverTriggered>(this);
            Game.Meta.Events.Subscribe<BuildingConstructionStarted>(this);

            FocusReset?.Invoke();
        }

        public override void Terminate()
        {
            NotificationsUI.Terminate();
            ActionBar.Terminate();
            TechnologyUI.Terminate();
            if (debugOverlay != null)
            {
                debugOverlay.Terminate();
                Navigation.Close(debugOverlay);
            }
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
            switch (UserSettings.Instance.Debug.GameDebugScreen)
            {
                case true when debugOverlay == null:
                {
                    debugOverlay = Navigation.Push<GameDebugOverlay>();
                    break;
                }
                case false when debugOverlay != null:
                {
                    debugOverlay.Terminate();
                    Navigation.Close(debugOverlay);
                    debugOverlay = null;
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
                .Add<BuildingStatusOverlay, IPlacedBuilding>(m => new BuildingStatusOverlayControl(m))
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
            switch (selectedObject)
            {
                case IPlacedBuilding building:
                    entityStatusNavigation!.ReplaceAll<BuildingStatusOverlay, IPlacedBuilding>(building);
                    break;
                // TODO: add a worker status screen
                default:
                    return;
            }

            EntityStatusOpened?.Invoke(selectedObject);
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

        public void HandleEvent(BuildingConstructionStarted @event)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (@event.Placeholder.SelectionState)
            {
                case SelectionState.Focused:
                    Game.SelectionManager.FocusObject(@event.Building);
                    break;
                case SelectionState.Selected:
                    Game.SelectionManager.SelectObject(@event.Building);
                    break;
            }
        }

        public void OnReturnToMainMenuButtonClicked()
        {
            runner.Shutdown();
            Navigation.Replace<MainMenu>(this);
            GameLeft?.Invoke();
        }
    }
}
