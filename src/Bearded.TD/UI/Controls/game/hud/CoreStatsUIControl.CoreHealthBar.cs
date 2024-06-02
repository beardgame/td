using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls;

sealed partial class CoreStatsUIControl
{
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
}
