using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using static Bearded.TD.Constants.UI;

namespace Bearded.TD.UI.Controls;

sealed class CoreStatsUIControl : CompositeControl
{
    private const double healthBarHeight = Constants.UI.Button.Height;
    private const double waveStatsWidth = Constants.UI.Button.Width + 2 * LayoutMarginSmall;

    public CoreStatsUIControl(CoreStatsUI model, UIContext uiContext)
    {
        IsClickThrough = true;
        this.BindIsVisible(model.Visible);

        this.BuildLayout()
            .ForContentBox()
            .DockFixedSizeToTop(new CoreHealthBar(model.Health), healthBarHeight)
            .FillContent(new GamePhaseAwareStatus(model, uiContext));
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

    private sealed class CoreHealthBar : CompositeControl
    {
        private static readonly Color healthColor = Color.Green;
        private static readonly Color remainderColor = Color.DimGray;
        private static readonly Color lostHealthColor = Color.Lerp(healthColor, remainderColor, 0.8f);

        private readonly Control healthControl;
        private readonly Control lostHealthControl;
        private readonly Control remainderControl;

        public CoreHealthBar(Binding<CoreStatsUI.CoreHealthStats> health)
        {
            healthControl = new BackgroundBox(healthColor);
            lostHealthControl = new BackgroundBox(lostHealthColor);
            remainderControl = new BackgroundBox(remainderColor);
            Add(healthControl);
            Add(lostHealthControl);
            Add(remainderControl);
            Add(new Border());

            updateValues(health.Value);
            health.SourceUpdated += updateValues;

            var transformedBinding = health.Transform(stats =>
                $"{stats.CurrentHealth.NumericValue} / {stats.MaxHealth.NumericValue}");
            Add(TextFactories.Label(transformedBinding, Label.TextAnchorCenter));
        }

        private void updateValues(CoreStatsUI.CoreHealthStats stats)
        {
            updateAnchors(stats);
        }

        private void updateAnchors(CoreStatsUI.CoreHealthStats stats)
        {
            var healthPercentage = stats.MaxHealth > HitPoints.Zero ? (stats.CurrentHealth / stats.MaxHealth) : 0;
            var waveStartHealthPercentage =
                stats.MaxHealth > HitPoints.Zero ? (stats.HealthAtWaveStart / stats.MaxHealth) : 0;

            var firstSplit = new Anchor(healthPercentage, 0);
            var secondSplit = new Anchor(waveStartHealthPercentage, 0);

            healthControl.SetAnchors(
                new Anchors(new Anchor(0, 0), firstSplit).H,
                healthControl.VerticalAnchors);
            lostHealthControl.SetAnchors(
                new Anchors(firstSplit, secondSplit).H,
                lostHealthControl.VerticalAnchors);
            remainderControl.SetAnchors(
                new Anchors(secondSplit, new Anchor(1, 0)).H,
                remainderControl.VerticalAnchors);
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }

    private sealed class GamePhaseAwareStatus : CompositeControl
    {
        public GamePhaseAwareStatus(CoreStatsUI model, UIContext uiContext)
        {
            IsClickThrough = true;

            var upcomingWaveInfo = new UpcomingWaveInformation(model.UpcomingWave, model.SkipWaveTimer, uiContext);
            Add(upcomingWaveInfo
                .Anchor(a => a.HorizontallyCentered(width: waveStatsWidth).Top(height: upcomingWaveInfo.Height))
                .BindIsVisible(model.CurrentPhase.Transform(phase => phase == CoreStatsUI.GamePhase.BetweenWaves)));
            Add(new EMP(model.EMPAvailable, model.FireEMP, uiContext)
                .Anchor(a =>
                    a.HorizontallyCentered(width: Constants.UI.Button.Width).Top(height: Constants.UI.Button.Height))
                .BindIsVisible(model.CurrentPhase.Transform(phase => phase == CoreStatsUI.GamePhase.InWave)));
        }
    }

    private sealed class UpcomingWaveInformation : CompositeControl
    {
        private readonly double contentHeight;
        public double Height => contentHeight + 2 * LayoutMarginSmall;

        public UpcomingWaveInformation(
            IReadonlyBinding<CoreStatsUI.UpcomingWaveCountdown?> upcomingWave,
            VoidEventHandler skipWaveTimer,
            UIContext uiContext)
        {
            Add(new BackgroundBox());

            var content = new CompositeControl();
            var column = content.BuildFixedColumn();
            column
                .AddLabel(upcomingWave.Transform(stats => stats?.Name ?? "<none>"), textAnchor: Label.TextAnchorCenter)
                .AddLabel(
                    upcomingWave.Transform(stats => stats?.TimeLeft?.ToDisplayString() ?? "-"), Label.TextAnchorCenter)
                .AddCenteredButton(
                    uiContext.Factories,
                    b => b
                        .WithLabel("Summon Wave")
                        .WithOnClick(skipWaveTimer)
                        .WithEnabled(upcomingWave.Transform(stats => stats?.CanSkip ?? false)));
            contentHeight = column.Height;

            this.BuildLayout().ForInnerContent().FillContent(content);
        }
    }

    private sealed class EMP : CompositeControl
    {
        public EMP(IReadonlyBinding<bool> isAvailable, VoidEventHandler activateEMP, UIContext uiContext)
        {
            Add(new BackgroundBox());
            Add(uiContext.Factories.Button(b => b
                .WithLabel("EMP")
                .WithEnabled(isAvailable)
                .WithOnClick(activateEMP)));
        }
    }
}
