using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using static Bearded.TD.Constants.UI;
using static Bearded.TD.Constants.UI.Window;

namespace Bearded.TD.UI.Controls;

sealed partial class CoreStatsUIControl : CompositeControl
{
    private const double healthBarHeight = Constants.UI.Button.Height;
    private const double empBoxWidth = Constants.UI.Button.Width + 2 * LayoutMarginSmall;
    private const double empBoxHeight = Constants.UI.Button.Height + 2 * LayoutMarginSmall;
    private const double waveStatsWidth = Constants.UI.Button.Width + 2 * LayoutMarginSmall;
    public const double Width = 640;
    public const double Height = healthBarHeight + empBoxHeight + 3 * LayoutMargin;

    public CoreStatsUIControl(CoreStatsUI model, UIContext uiContext)
    {
        IsClickThrough = true;
        this.BindIsVisible(model.Visible);

        this.BuildLayout()
            .ForContentBox()
            .DockFixedSizeToLeft(new StaticWaveInformation(model.Wave), waveStatsWidth)
            .DockFixedSizeToRight(
                new UpcomingWaveInformation(model.Wave, model.CurrentPhase, model.SkipWaveTimer, uiContext),
                waveStatsWidth)
            .DockFixedSizeToTop(new CoreHealthBar(model.Health), healthBarHeight)
            .DockFixedSizeToTop(
                new EMP(model.EMPAvailable, model.FireEMP, uiContext)
                    .WrapHorizontallyCentered(empBoxWidth)
                    .BindIsVisible(model.CurrentPhase.Transform(phase => phase == CoreStatsUI.GamePhase.InWave)),
                empBoxHeight);
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

    private sealed class StaticWaveInformation : CompositeControl
    {
        public StaticWaveInformation(
            IReadonlyBinding<CoreStatsUI.WaveState?> wave)
        {
            this.Add(createWaveInfoBackground());

            var content = CreateClickThrough();
            var column = content.BuildFixedColumn();
            column.AddLabel(wave.Transform(w => w?.Name ?? "<none>"), textAnchor: Label.TextAnchorCenter);

            Add(content.Anchor(a => a.VerticallyCentered(column.Height)));
        }
    }

    private sealed class UpcomingWaveInformation : CompositeControl
    {
        public UpcomingWaveInformation(
            IReadonlyBinding<CoreStatsUI.WaveState?> wave,
            IReadonlyBinding<CoreStatsUI.GamePhase> phase,
            VoidEventHandler skipWaveTimer,
            UIContext uiContext)
        {
            this.Add(createWaveInfoBackground());

            // TODO: add information about upcoming enemies

            var button = uiContext.Factories.Button(b => b
                .WithLabel(wave.Transform(stats => stats?.TimeLeft?.ToDisplayString() ?? "Summon"))
                .WithOnClick(skipWaveTimer)
                .WithEnabled(wave.Transform(stats => stats?.CanSkip ?? false)));
            button.BindIsVisible(phase.Transform(p => p == CoreStatsUI.GamePhase.BetweenWaves));
            Add(button.Anchor(a => a
                .Left(LayoutMarginSmall)
                .Right(LayoutMarginSmall)
                .Bottom(LayoutMarginSmall, Constants.UI.Button.Height)));
        }
    }

    private sealed class EMP : CompositeControl
    {
        public EMP(IReadonlyBinding<bool> isAvailable, VoidEventHandler activateEMP, UIContext uiContext)
        {
            Add(uiContext.Factories.Button(b => b
                    .WithLabel("EMP")
                    .WithEnabled(isAvailable)
                    .WithOnClick(activateEMP)
                    .WithShadow()
                    .WithBlurredBackground())
                .Anchor(a => a.Centered(Constants.UI.Button.Width, Constants.UI.Button.Height)));
        }
    }

    private static Control[] createWaveInfoBackground() =>
        new ComplexBox
        {
            CornerRadius = CornerRadius,
            Components = BackgroundComponents,
        }.WithDecorations(new Decorations(
            Shadow: Shadow,
            BlurredBackground: BlurredBackground.Default
        ));
}
