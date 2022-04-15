using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;

namespace Bearded.TD.UI.Controls;

sealed class GameUIControl : CompositeControl
{
    private readonly GameUI gameUI;

    public GameUIControl(GameUI gameUI, RenderContext renderContext)
    {
        GameWorldControl gameWorldControl;
        this.gameUI = gameUI;

        CanBeFocused = true;

        Add(gameWorldControl = new GameWorldControl(gameUI.Game, renderContext, gameUI.TimeSource));

        var nonDiegeticUIWrapper = CreateClickThrough();
        nonDiegeticUIWrapper.BindIsVisible(gameUI.GameUIController.NonDiegeticUIVisibility);
        nonDiegeticUIWrapper.Add(new ActionBarControl(gameUI.ActionBar)
            .Anchor(a => a
                .Left(width: 160)
                .Top(margin: -200, height: 400, relativePercentage: .5))
            .BindIsVisible(gameUI.GameUIController.ActionBarVisibility));
        var gameStatusControl = new GameStatusUIControl(gameUI.GameUIController, gameUI.GameStatusUI)
            .Anchor(a => a
                .Right(width: 200)
                .Top(margin: 0, height: 220));
        nonDiegeticUIWrapper.Add(gameStatusControl);
        nonDiegeticUIWrapper.Add(new PlayerStatusUIControl(gameUI.PlayerStatusUI)
            .Anchor(a => a
                .Right(width: 200)
                .Below(gameStatusControl, height: 100)));
        Add(nonDiegeticUIWrapper);

        Add(new GameMenuControl()
            .Subscribe(ctrl => ctrl.ResumeGameButtonClicked += () => ctrl.IsVisible = false)
            .Subscribe(ctrl => ctrl.ReturnToMainMenuButtonClicked += gameUI.OnReturnToMainMenuButtonClicked)
            .BindIsVisible(gameUI.GameUIController.GameMenuVisibility));

        Add(new TechnologyUIControl(gameUI.GameUIController, gameUI.TechnologyUI)
            .Anchor(a => a.MarginAllSides(80))
            .BindIsVisible(gameUI.GameUIController.TechnologyModalVisibility));

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
        keyEventArgs.Handled = gameUI.GameUIController.TryHandleKeyHit(keyEventArgs);
        base.KeyHit(keyEventArgs);
    }

    private void onGameOver()
    {
        Add(new GameEndControl("you lose")
            .Anchor(a => a
                .Top(margin: 0, height: 64)
                .Left(relativePercentage: .5, margin: -120, width: 240))
            .Subscribe(ctrl => ctrl.ReturnToMainMenuButtonClicked += gameUI.OnReturnToMainMenuButtonClicked));
    }

    private void onGameVictory()
    {
        Add(new GameEndControl("you win")
            .Anchor(a => a
                .Top(margin: 0, height: 64)
                .Left(relativePercentage: .5, margin: -120, width: 240))
            .Subscribe(ctrl => ctrl.ReturnToMainMenuButtonClicked += gameUI.OnReturnToMainMenuButtonClicked));
    }
}
