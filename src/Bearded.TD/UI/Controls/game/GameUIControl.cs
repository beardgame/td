using System;
using Bearded.TD.Content;
using Bearded.TD.Game.Commands;
using Bearded.TD.Rendering;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

sealed class GameUIControl : CompositeControl
{
    private const double technologyButtonSize = 96;

    private readonly GameUI gameUI;
    private readonly Animations animations;

    public GameUIControl(
        GameUI gameUI, ContentManager contentManager, RenderContext renderContext, Animations animations)
    {
        var gameWorldControl = new GameWorldControl(gameUI.Game, renderContext, gameUI.TimeSource);
        var gameWorldOverlay = new GameWorldOverlay(
            gameUI.Game.Camera,
            animations,
            gameUI.Tooltips,
            new GameRequestDispatcher(gameUI.Game),
            contentManager,
            gameUI.Game.Meta.SoundScape);

        this.gameUI = gameUI;
        this.animations = animations;

        CanBeFocused = true;

        Add(gameWorldControl);

        var nonDiegeticUIWrapper = CreateClickThrough();
        nonDiegeticUIWrapper.BindIsVisible(gameUI.GameUIController.NonDiegeticUIVisibility);
        nonDiegeticUIWrapper.Add(new ActionBarControl(gameUI.ActionBar, gameUI.Tooltips)
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

        var techButton = ButtonFactories.StandaloneIconButton(b => b
            .WithIcon(Constants.Content.CoreUI.Sprites.Technology)
            .MakeHexagon()
            .WithShadow()
            .WithBlurredBackground()
            .WithBackgroundColors(Constants.UI.Button.DefaultBackgroundColors * 0.8f)
            .WithOnClick(gameUI.GameUIController.ShowTechnologyModal));

        techButton.Add(techButtonGlow());

        this.BuildLayout()
            .ForFullScreen()
            .DockFixedSizeToTop(
                techButton.WrapAligned(technologyButtonSize, technologyButtonSize, 1, 0.5),
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

    private Control techButtonGlow()
    {
        var components = new ShapeComponent[1];
        var control = new ComplexHexagon
        {
            IsVisible = false,
            Components = ShapeComponents.FromMutable(components),
            CornerRadius = 24,
        };

        var animation = AnimationFunction.ZeroToOneRepeat(1.S(), t =>
            {
                var a = 0.5f - 0.5f * MathF.Cos(t * MathF.Tau);
                var color = Constants.UI.Colors.TechButtonGlow * a;
                var glow = GradientDefinition.SimpleGlow(color).AddFlags(GradientFlags.ExtendNegative);
                components[0] = new ShapeComponent(-24, 24, glow);
                control.IsVisible = true;
            },
            end: () => control.IsVisible = false,
            whileTrue: () => gameUI.TechTokenIsAvailable.Value
        );

        gameUI.TechTokenIsAvailable.SourceUpdated += available =>
        {
            if (available && !control.IsVisible)
                animations.Start(animation);
        };

        return control;
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
