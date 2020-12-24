using Bearded.TD.Rendering;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.UI.Controls
{
    sealed class GameUIControl : CompositeControl
    {
        private readonly GameUI gameUI;
        private readonly GamePausedControl gamePausedControl;
        private readonly TechnologyUIControl technologyUIControl;

        public GameUIControl(GameUI gameUI, RenderContext renderContext)
        {
            GameWorldControl gameWorldControl;
            this.gameUI = gameUI;

            CanBeFocused = true;

            Add(gameWorldControl = new GameWorldControl(gameUI.Game, renderContext));
            Add(new GameNotificationsUIControl(gameUI.NotificationsUI)
                .Anchor(a => a.Left(margin: 0, width: 320))); /* Vertical anchors managed dynamically. */
            Add(new ActionBarControl(gameUI.ActionBar)
                .Anchor(a => a
                    .Left(width: 160)
                    .Top(margin: -200, height: 400, relativePercentage: .5)));
            var gameStatusControl = new GameStatusUIControl(gameUI.GameStatusUI)
                .Anchor(a => a
                    .Right(width: 200)
                    .Top(margin: 0, height: 180))
                .Subscribe(ctrl => ctrl.TechnologyButtonClicked += () => technologyUIControl.IsVisible = true);
            Add(gameStatusControl);
            var playerStatusControl = new PlayerStatusUIControl(gameUI.PlayerStatusUI)
                .Anchor(a => a
                    .Right(width: 200)
                    .Below(gameStatusControl, height: 100));
            Add(playerStatusControl);
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
                    gamePausedControl.IsVisible = !gamePausedControl.IsVisible;
                    break;
                default:
                    return false;
            }
            return true;
        }

        private void onGameOver()
        {
            Add(new GameOverControl("you lose")
                .Anchor(a => a
                    .Top(margin: 0, height: 64)
                    .Left(relativePercentage: .5, margin: - 120, width: 240))
                .Subscribe(ctrl => ctrl.ReturnToMainMenuButtonClicked += gameUI.OnReturnToMainMenuButtonClicked));
        }

        private void onGameVictory()
        {
            Add(new GameOverControl("you win")
                .Anchor(a => a
                    .Top(margin: 0, height: 64)
                    .Left(relativePercentage: .5, margin: - 120, width: 240))
                .Subscribe(ctrl => ctrl.ReturnToMainMenuButtonClicked += gameUI.OnReturnToMainMenuButtonClicked));
        }
    }
}
