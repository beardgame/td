﻿using amulware.Graphics;
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

namespace Bearded.TD.UI.Controls
{
    sealed class GameUI : UpdateableNavigationNode<(GameInstance game, GameRunner runner)>,
        IListener<GameOverTriggered>, IListener<BuildingConstructionStarted>
    {
        public GameInstance Game { get; private set; }
        private GameRunner runner;
        private InputManager inputManager;
        private FocusManager focusManager;

        public ActionBar ActionBar { get; }
        public GameStatusUI GameStatusUI { get; }
        public TechnologyUI TechnologyUI { get; }

        public event VoidEventHandler FocusReset;
        public event GenericEventHandler<ISelectable> EntityStatusOpened;
        public event VoidEventHandler EntityStatusClosed;
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
            focusManager = dependencies.Resolve<FocusManager>();

            ActionBar.Initialize(Game);
            GameStatusUI.Initialize(Game);
            TechnologyUI.Initialize(Game);

            Game.SelectionManager.ObjectSelected += onObjectSelected;
            Game.SelectionManager.ObjectDeselected += onObjectDeselected;
            Game.Meta.Events.Subscribe<GameOverTriggered>(this);
            Game.Meta.Events.Subscribe<BuildingConstructionStarted>(this);
        }

        public override void Terminate()
        {
            ActionBar.Terminate();
            TechnologyUI.Terminate();
            base.Terminate();
        }

        public override void Update(UpdateEventArgs args)
        {
            if (focusManager.FocusedControl == null)
            {
                FocusReset?.Invoke();
            }

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

        public void OnReturnToMainMenuButtonClicked()
        {
            runner.Shutdown();
            Navigation.Replace<MainMenu>(this);
            GameLeft?.Invoke();
        }
    }
}
