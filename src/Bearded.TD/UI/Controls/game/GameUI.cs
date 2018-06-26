﻿using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Utilities.Input;
using Bearded.UI.Navigation;
using Bearded.Utilities.Input;

namespace Bearded.TD.UI.Controls
{
    class GameUI : UpdateableNavigationNode<(GameInstance game, GameRunner runner)>
    {
        public GameInstance Game { get; private set; }
        private GameRunner runner;
        private InputManager inputManager;

        public ActionBar ActionBar { get; }
        public GameStatusUI GameStatusUI { get; }
        
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
        }

        public override void Update(UpdateEventArgs args)
        {
            var inputState = new InputState(new List<char>(), inputManager);
            
            runner.HandleInput(args, inputState);
            runner.Update(args);

            GameStatusUI.Update();
        }
    }
}
