using Bearded.TD.Rendering;
using Bearded.UI.Controls;

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
            .BindIsVisible(gameUI.GameUIController.ActionBarVisibility));
        var gameStatusControl = new GameStatusUIControl(gameUI.GameUIController, gameUI.GameStatusUI)
            .Anchor(a => a
                .Right(width: 200)
                .Top(margin: 0, height: 220));
        nonDiegeticUIWrapper.Add(gameStatusControl);
        nonDiegeticUIWrapper.Add(new CoreStatsUIControl(gameUI.CoreStats)
            .Anchor(a => a
                .Top(height: 480)
                .Left(margin: -240, width: 480, relativePercentage: .5)));
        Add(nonDiegeticUIWrapper);

        Add(new GameMenuControl()
            .Subscribe(ctrl => ctrl.ResumeGameButtonClicked += gameUI.OnResumeGameButtonClicked)
            .Subscribe(ctrl => ctrl.ReturnToMainMenuButtonClicked += gameUI.OnReturnToMainMenuButtonClicked)
            .BindIsVisible(gameUI.GameUIController.GameMenuVisibility));

        Add(new TechnologyWindowControl(gameUI.TechnologyUI)
            .BindIsVisible(gameUI.GameUIController.TechnologyModalVisibility));

        var overlayControl = CreateClickThrough();
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
