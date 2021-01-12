using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.UI.Controls
{
    sealed class GameUIControl : CompositeControl
    {
        private readonly Binding<bool> isEntityStatusOpen = new(false);
        private readonly Binding<bool> isGameRunning = new(true);

        private readonly GameUI gameUI;
        private readonly GamePausedControl gamePausedControl;
        private readonly TechnologyUIControl technologyUIControl;

        public GameUIControl(GameUI gameUI, RenderContext renderContext)
        {
            GameWorldControl gameWorldControl;
            this.gameUI = gameUI;

            CanBeFocused = true;

            Add(gameWorldControl = new GameWorldControl(gameUI.Game, renderContext));
            Add(new ActionBarControl(gameUI.ActionBar)
                .Anchor(a => a
                    .Left(width: 160)
                    .Top(margin: -200, height: 400, relativePercentage: .5))
                .BindIsVisible(Binding.Combine(
                    isGameRunning, isEntityStatusOpen,
                    (gameRunning, entityStatusOpen) => gameRunning && !entityStatusOpen)));
            var gameStatusControl = new GameStatusUIControl(gameUI.GameStatusUI)
                .Anchor(a => a
                    .Right(width: 200)
                    .Top(margin: 0, height: 180))
                .Subscribe(ctrl => ctrl.TechnologyButtonClicked += () => technologyUIControl.IsVisible = true)
                .BindIsVisible(isGameRunning);
            Add(gameStatusControl);
            Add(new PlayerStatusUIControl(gameUI.PlayerStatusUI)
                .Anchor(a => a
                    .Right(width: 200)
                    .Below(gameStatusControl, height: 100))
                .BindIsVisible(isGameRunning));

            gameUI.EntityStatusOpened += _ => isEntityStatusOpen.SetFromSource(true);
            gameUI.EntityStatusClosed += () => isEntityStatusOpen.SetFromSource(false);

            gamePausedControl = new GamePausedControl {IsVisible = false}
                .Subscribe(ctrl => ctrl.ResumeGameButtonClicked += () => ctrl.IsVisible = false)
                .Subscribe(ctrl => ctrl.ReturnToMainMenuButtonClicked += gameUI.OnReturnToMainMenuButtonClicked);
            Add(gamePausedControl);

            technologyUIControl = new TechnologyUIControl(gameUI.TechnologyUI) {IsVisible = false}
                .Anchor(a => a
                    .Top(margin: 80)
                    .Bottom(margin: 80)
                    .Right(margin: 80)
                    .Left(margin: 80))
                .Subscribe(ctrl => ctrl.CloseButtonClicked += () => ctrl.IsVisible = false);
            Add(technologyUIControl);

            var overlayControl = new Overlay();
            Add(overlayControl);
            gameUI.SetOverlayControl(overlayControl);

            Add(new GameNotificationsUIControl(gameUI.NotificationsUI)
                .Anchor(a => a.Left(margin: 0, width: 320))); /* Vertical anchors managed dynamically. */

            gameUI.FocusReset += Focus;
            gameUI.GameLeft += Unfocus;
            gameUI.GameOverTriggered += onGameOver;
            gameUI.GameVictoryTriggered += onGameVictory;
            gameUI.GameLeft += gameWorldControl.CleanUp;
        }

        public override void KeyHit(KeyEventArgs keyEventArgs)
        {
            keyEventArgs.Handled = tryHandleKeyHit(keyEventArgs.Key);
            base.KeyHit(keyEventArgs);
        }

        private bool tryHandleKeyHit(Keys key)
        {
            switch (key)
            {
                case Keys.T:
                    technologyUIControl.IsVisible = !technologyUIControl.IsVisible;
                    break;
                case Keys.Escape:
                    if (technologyUIControl.IsVisible)
                    {
                        technologyUIControl.IsVisible = false;
                    }
                    else if (isGameRunning.Value)
                    {
                        onGamePause();
                    }
                    else
                    {
                        onGameResume();
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        private void onGamePause()
        {
            isGameRunning.SetFromSource(false);
            gameUI.Game.SelectionManager.ResetSelection();
            gamePausedControl.IsVisible = true;
        }

        private void onGameResume()
        {
            isGameRunning.SetFromSource(true);
            gamePausedControl.IsVisible = false;
        }

        private void onGameOver()
        {
            Add(new GameEndControl("you lose")
                .Anchor(a => a
                    .Top(margin: 0, height: 64)
                    .Left(relativePercentage: .5, margin: - 120, width: 240))
                .Subscribe(ctrl => ctrl.ReturnToMainMenuButtonClicked += gameUI.OnReturnToMainMenuButtonClicked));
        }

        private void onGameVictory()
        {
            Add(new GameEndControl("you win")
                .Anchor(a => a
                    .Top(margin: 0, height: 64)
                    .Left(relativePercentage: .5, margin: - 120, width: 240))
                .Subscribe(ctrl => ctrl.ReturnToMainMenuButtonClicked += gameUI.OnReturnToMainMenuButtonClicked));
        }
    }
}
