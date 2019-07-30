using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Events;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Workers;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Input;
using Bearded.UI.Controls;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using Bearded.Utilities.Input;
using OpenTK.Input;

namespace Bearded.TD.UI.Controls
{
    sealed class GameUI : UpdateableNavigationNode<(GameInstance game, GameRunner runner)>,
        IListener<GameOverTriggered>, IListener<BuildingConstructionStarted>
    {
        public GameInstance Game { get; private set; }
        private GameRunner runner;
        private InputManager inputManager;

        public ActionBar ActionBar { get; }
        public GameStatusUI GameStatusUI { get; }
        public TechnologyUI TechnologyUI { get; }

        private bool isGameMenuOpen = false;
        private bool isTechnologyScreenOpen = false;

        public event GenericEventHandler<ISelectable> EntityStatusOpened;
        public event VoidEventHandler EntityStatusClosed;
        public event VoidEventHandler GameMenuOpened;
        public event VoidEventHandler GameMenuClosed;
        public event VoidEventHandler TechnologyScreenOpened;
        public event VoidEventHandler TechnologyScreenClosed;
        public event VoidEventHandler GameOverTriggered;
        public event VoidEventHandler GameLeft;

        private NavigationController entityStatusNavigation;

        public GameUI()
        {
            ActionBar = new ActionBar();
            GameStatusUI = new GameStatusUI();
            TechnologyUI = new TechnologyUI();
        }

        protected override void Initialize(
            DependencyResolver dependencies, (GameInstance game, GameRunner runner) parameters)
        {
            base.Initialize(dependencies, parameters);

            Game = parameters.game;
            runner = parameters.runner;

            inputManager = dependencies.Resolve<InputManager>();

            ActionBar.Initialize(Game);
            GameStatusUI.Initialize(Game);
            TechnologyUI.Initialize(Game);

            GameStatusUI.TechnologyButtonClicked += openTechnologyScreen;
            TechnologyUI.CloseButtonClicked += closeTechnologyScreen;

            Game.SelectionManager.ObjectSelected += onObjectSelected;
            Game.SelectionManager.ObjectDeselected += onObjectDeselected;
            Game.Meta.Events.Subscribe<GameOverTriggered>(this);
            Game.Meta.Events.Subscribe<BuildingConstructionStarted>(this);
        }

        public override void Terminate()
        {
            TechnologyUI.Terminate();
            base.Terminate();
        }

        public override void Update(UpdateEventArgs args)
        {
            updateGameMenuVisibility();

            var inputState = new InputState(inputManager);

            runner.HandleInput(args, inputState);
            runner.Update(args);

            GameStatusUI.Update();
        }

        public void SetEntityStatusContainer(IControlParent controlParent)
        {
            DebugAssert.State.Satisfies(entityStatusNavigation == null, "Can only initialize entity status UI once.");

            var dependencies = new DependencyResolver();
            dependencies.Add(Game);

            var (models, views) = NavigationFactories.ForBoth()
                .Add<BuildingStatusUI, IPlacedBuilding>(m => new BuildingStatusUIControl(m))
                .Add<WorkerStatusUI, Faction>(m => new WorkerStatusUIControl(m))
                .ToDictionaries();

            entityStatusNavigation = new NavigationController(
                controlParent,
                dependencies,
                models,
                views);
            entityStatusNavigation.Exited += Game.SelectionManager.ResetSelection;
        }

        private void onObjectSelected(ISelectable selectedObject)
        {
            switch (selectedObject)
            {
                case IPlacedBuilding building:
                    entityStatusNavigation.ReplaceAll<BuildingStatusUI, IPlacedBuilding>(building);
                    break;
                case Worker worker:
                    entityStatusNavigation.ReplaceAll<WorkerStatusUI, Faction>(worker.Faction);
                    break;
                default:
                    return;
            }

            EntityStatusOpened?.Invoke(selectedObject);
        }

        private void onObjectDeselected(ISelectable t)
        {
            entityStatusNavigation.CloseAll();
            EntityStatusClosed?.Invoke();
        }

        public void HandleEvent(GameOverTriggered @event)
        {
            GameOverTriggered?.Invoke();
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

        private void updateGameMenuVisibility()
        {
            if (inputManager.IsKeyHit(Key.Escape))
            {
                if (isGameMenuOpen)
                {
                    closeGameMenu();
                }
                else
                {
                    openGameMenu();
                }
            }
        }

        public void OnCloseGameMenuButtonClicked()
        {
            closeGameMenu();
        }

        private void openGameMenu()
        {
            GameMenuOpened?.Invoke();
            isGameMenuOpen = true;
        }

        private void closeGameMenu()
        {
            isGameMenuOpen = false;
            GameMenuClosed?.Invoke();
        }

        private void openTechnologyScreen()
        {
            if (isTechnologyScreenOpen) return;

            TechnologyScreenOpened?.Invoke();
            isTechnologyScreenOpen = true;
        }

        private void closeTechnologyScreen()
        {
            if (!isTechnologyScreenOpen) return;

            isTechnologyScreenOpen = false;
            TechnologyScreenClosed?.Invoke();
        }

        public void OnReturnToMainMenuButtonClicked()
        {
            runner.Shutdown();
            Navigation.Replace<MainMenu>(this);
            GameLeft?.Invoke();
        }
    }
}
