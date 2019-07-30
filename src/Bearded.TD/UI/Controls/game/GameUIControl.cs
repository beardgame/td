﻿using Bearded.TD.Rendering;
using Bearded.UI;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed class GameUIControl : CompositeControl
    {
        private readonly GameUI gameUI;
        private readonly GameWorldControl gameWorldControl;
        private readonly GamePausedControl gamePausedControl;
        private readonly TechnologyUIControl technologyUIControl;

        public GameUIControl(GameUI gameUI, RenderContext renderContext)
        {
            this.gameUI = gameUI;

            Add(gameWorldControl = new GameWorldControl(gameUI.Game, renderContext));
            Add(new ActionBarControl(gameUI.ActionBar)
                .Anchor(a => a
                    .Left(width: 160)
                    .Top(margin: -200, height: 400, relativePercentage: .5)));
            Add(new GameStatusUIControl(gameUI.GameStatusUI)
                .Anchor(a => a
                    .Right(width: 200)
                    .Top(margin: 0, height: 92)));
            Add(new CompositeControl { IsVisible = false }
                .Anchor(a => a
                    .Right(width: 200)
                    .Bottom(height: 320))
                .Subscribe(gameUI.SetEntityStatusContainer)
                .Subscribe(container => gameUI.EntityStatusClosed += () => container.IsVisible = false)
                .Subscribe(container => gameUI.EntityStatusOpened += _ => container.IsVisible = true));

            gamePausedControl = new GamePausedControl {IsVisible = false}
                .Anchor(a => a
                    .Top(margin: 0, height: 96)
                    .Left(relativePercentage: .5, margin: -120, width: 240))
                .Subscribe(ctrl => ctrl.ResumeGameButtonClicked += gameUI.OnCloseGameMenuButtonClicked)
                .Subscribe(ctrl => ctrl.ReturnToMainMenuButtonClicked += gameUI.OnReturnToMainMenuButtonClicked);
            Add(gamePausedControl);

            technologyUIControl = new TechnologyUIControl(gameUI.TechnologyUI) {IsVisible = false}
                .Anchor(a => a
                    .Top(margin: 80)
                    .Bottom(margin: 80)
                    .Right(margin: 80)
                    .Left(margin: 80)
                );
            Add(technologyUIControl);

            gameUI.GameMenuOpened += onGameMenuOpened;
            gameUI.GameMenuClosed += onGameMenuClosed;
            gameUI.TechnologyScreenOpened += onTechnologyOpened;
            gameUI.TechnologyScreenClosed += onTechnologyClosed;
            gameUI.GameOverTriggered += onGameOver;
            gameUI.GameLeft += gameWorldControl.CleanUp;
        }

        private void onGameMenuOpened()
        {
            gamePausedControl.IsVisible = true;
        }

        private void onGameMenuClosed()
        {
            gamePausedControl.IsVisible = false;
        }

        private void onTechnologyOpened()
        {
            technologyUIControl.IsVisible = true;
        }

        private void onTechnologyClosed()
        {
            technologyUIControl.IsVisible = false;
        }

        private void onGameOver()
        {
            Add(new GameOverControl()
                .Anchor(a => a
                    .Top(margin: 0, height: 64)
                    .Left(relativePercentage: .5, margin: - 120, width: 240))
                .Subscribe(ctrl => ctrl.ReturnToMainMenuButtonClicked += gameUI.OnReturnToMainMenuButtonClicked));
        }
    }
}
