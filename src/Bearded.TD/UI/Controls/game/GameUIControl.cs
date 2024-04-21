using Bearded.TD.Game.Commands;
using Bearded.TD.Rendering;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Factories;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

sealed class GameUIControl : CompositeControl
{
    private const double technologyButtonSize = 128;

    private readonly GameUI gameUI;

    public GameUIControl(GameUI gameUI, RenderContext renderContext, Animations animations)
    {
        var gameWorldControl = new GameWorldControl(gameUI.Game, renderContext, gameUI.TimeSource);
        var gameWorldOverlay = new GameWorldOverlay(
            gameUI.Game.Camera,
            animations,
            gameUI.Tooltips,
            new GameRequestDispatcher(gameUI.Game));

        this.gameUI = gameUI;

        CanBeFocused = true;

        Add(gameWorldControl);

        var nonDiegeticUIWrapper = CreateClickThrough();
        nonDiegeticUIWrapper.BindIsVisible(gameUI.GameUIController.NonDiegeticUIVisibility);
        nonDiegeticUIWrapper.Add(new ActionBarControl(gameUI.ActionBar)
            .BindIsVisible(gameUI.GameUIController.ActionBarVisibility));
        nonDiegeticUIWrapper.Add(new CoreStatsUIControl(gameUI.CoreStats)
            .Anchor(a => a
                .Top(height: 480)
                .Left(margin: -240, width: 480, relativePercentage: .5)));
        nonDiegeticUIWrapper.Add(gameWorldOverlay);
        Add(nonDiegeticUIWrapper);

        Add(new StatisticsSideBarControl(gameUI.StatisticsSideBar, animations).Anchor(a => a
            .Top(technologyButtonSize + 8 * Constants.UI.Button.Margin)
            .Bottom(Constants.UI.Button.SquareButtonSize + 4 * Constants.UI.Button.Margin)
        ));

        this.BuildLayout()
            .ForFullScreen()
            .DockFixedSizeToTop(
                ButtonFactories.StandaloneIconButton(b => b
                        .WithIcon(Constants.Content.CoreUI.Sprites.Technology)
                        .MakeHexagon()
                        .WithShadow()
                        .WithOnClick(gameUI.GameUIController.ShowTechnologyModal))
                    .WrapAligned(technologyButtonSize, technologyButtonSize, 1, 0.5),
                technologyButtonSize + 4 * Constants.UI.Button.Margin);
        Add(new TechnologyWindowControl(gameUI.TechnologyUI)
            .BindIsVisible(gameUI.GameUIController.TechnologyModalVisibility));

        var overlayControl = CreateClickThrough();
        Add(overlayControl);
        gameUI.SetOverlayControl(overlayControl);
        gameUI.SetWorldOverlay(gameWorldOverlay);

        Add(new GameNotificationsUIControl(gameUI.NotificationsUI)
            .Anchor(a => a.Left(margin: 0, width: 320))); /* Vertical anchors managed dynamically. */

        Add(new GameMenuControl()
            .Subscribe(ctrl => ctrl.ResumeGameButtonClicked += gameUI.OnResumeGameButtonClicked)
            .Subscribe(ctrl => ctrl.ReturnToMainMenuButtonClicked += gameUI.OnReturnToMainMenuButtonClicked)
            .BindIsVisible(gameUI.GameUIController.GameMenuVisibility));

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
