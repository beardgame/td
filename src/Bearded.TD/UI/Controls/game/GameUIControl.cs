using Bearded.TD.Rendering;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using OpenTK.Input;

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
            Add(new ActionBarControl(gameUI.ActionBar)
                .Anchor(a => a
                    .Left(width: 160)
                    .Top(margin: -200, height: 400, relativePercentage: .5)));
            var gameStatusControl = new GameStatusUIControl(gameUI.GameStatusUI)
                .Anchor(a => a
                    .Right(width: 200)
                    .Top(margin: 0, height: 92))
                .Subscribe(ctrl => ctrl.TechnologyButtonClicked += () => technologyUIControl.IsVisible = true);
            Add(gameStatusControl);
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
                .Subscribe(ctrl => ctrl.ResumeGameButtonClicked += () => gamePausedControl.IsVisible = false)
                .Subscribe(ctrl => ctrl.ReturnToMainMenuButtonClicked += gameUI.OnReturnToMainMenuButtonClicked);
            Add(gamePausedControl);

            technologyUIControl = new TechnologyUIControl(gameUI.TechnologyUI) {IsVisible = false}
                .Anchor(a => a
                    .Top(margin: 80)
                    .Bottom(margin: 80)
                    .Right(margin: 80)
                    .Left(margin: 80))
                .Subscribe(ctrl => ctrl.CloseButtonClicked += () => technologyUIControl.IsVisible = false);
            Add(technologyUIControl);

            gameUI.FocusReset += Focus;
            gameUI.GameOverTriggered += onGameOver;
            gameUI.GameLeft += gameWorldControl.CleanUp;
        }

        public override void KeyHit(KeyEventArgs keyEventArgs)
        {
            keyEventArgs.Handled = tryHandleKeyHit(keyEventArgs.Key);
        }

        private bool tryHandleKeyHit(Key key)
        {
            switch (key)
            {
                case Key.T:
                    technologyUIControl.IsVisible = !technologyUIControl.IsVisible;
                    break;
                case Key.Escape:
                    gamePausedControl.IsVisible = !gamePausedControl.IsVisible;
                    break;
                default:
                    return false;
            }
            return true;
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
