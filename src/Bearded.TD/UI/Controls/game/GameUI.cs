﻿using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Events;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Input;
using Bearded.UI.Controls;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using Bearded.Utilities.Input;
using OpenTK.Input;

namespace Bearded.TD.UI.Controls
{
    class GameUI : UpdateableNavigationNode<(GameInstance game, GameRunner runner)>,
        IListener<GameOverTriggered>
    {
        public GameInstance Game { get; private set; }
        private GameRunner runner;
        private InputManager inputManager;

        public ActionBar ActionBar { get; }
        public GameStatusUI GameStatusUI { get; }

        private bool isGameMenuOpen = false;
        
        public event GenericEventHandler<ISelectable> EntityStatusOpened;
        public event VoidEventHandler EntityStatusClosed;
        public event VoidEventHandler GameMenuOpened;
        public event VoidEventHandler GameMenuClosed;
        public event VoidEventHandler GameOverTriggered;

        private NavigationController entityStatusNavigation;
        
        public GameUI()
        {
            ActionBar = new ActionBar();
            GameStatusUI = new GameStatusUI();
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

            Game.SelectionManager.ObjectSelected += onObjectSelected;
            Game.SelectionManager.ObjectDeselected += onObjectDeselected;
            Game.Meta.Events.Subscribe(this);
        }

        public override void Update(UpdateEventArgs args)
        {
            updateGameMenuVisibility();
            
            var inputState = new InputState(new List<char>(), inputManager);

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
        
        private void updateGameMenuVisibility()
        {
            if (!inputManager.IsKeyHit(Key.Escape)) return;
            
            if (isGameMenuOpen)
            {
                closeGameMenu();
            }
            else
            {
                openGameMenu();
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
        
        public void OnReturnToMainMenuButtonClicked()
        {
            runner.Shutdown();
            Navigation.Replace<MainMenu>(this);
        }
    }
}
