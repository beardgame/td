using System;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Elements;

sealed partial class Capacitor
{
    private sealed class CapacitorStatus
    {
        private IStatusReceipt? status;
        private SpriteBuckets icons;
        private float chargePercentage;

        public CapacitorStatus(GameState game, IStatusTracker statusTracker)
        {
            var iconDrawer = new BucketedIconDrawer(
                ImmutableArray.Create(
                    iconStatusDrawer(game, "battery-0"),
                    iconStatusDrawer(game, "battery-25"),
                    iconStatusDrawer(game, "battery-50"),
                    iconStatusDrawer(game, "battery-75"),
                    iconStatusDrawer(game, "battery-100")),
                () => chargePercentage);
            var progressDrawer = new ProgressStatusDrawer(iconDrawer, () => chargePercentage);
            icons = new SpriteBuckets(
                ImmutableArray.Create(
                    "battery-0".ToStatusIconSpriteId(),
                    "battery-25".ToStatusIconSpriteId(),
                    "battery-50".ToStatusIconSpriteId(),
                    "battery-75".ToStatusIconSpriteId(),
                    "battery-100".ToStatusIconSpriteId()));
            status = statusTracker.AddStatus(
                new StatusSpec(StatusType.Neutral, null, progressDrawer),
                statusAppearance(),
                null);
        }

        private static IStatusDrawer iconStatusDrawer(GameState game, string iconName) =>
            IconStatusDrawer.FromSpriteBlueprint(game, game.Meta.Blueprints.LoadStatusIconSprite(iconName));

        public void UpdateCharge(ElectricCharge currentCharge, ElectricCharge maxCharge)
        {
            chargePercentage = currentCharge / maxCharge;
            status?.UpdateAppearance(statusAppearance());
        }

        private StatusAppearance statusAppearance()
        {
            return StatusAppearance.IconAndProgress(icons.ResolveIcon(chargePercentage), chargePercentage);
        }

        public void Detach()
        {
            status?.DeleteImmediately();
            status = null;
        }
    }

    private sealed class BucketedIconDrawer : IStatusDrawer
    {
        private readonly ImmutableArray<IStatusDrawer> drawers;
        private readonly Func<float> getPercentage;

        private int index
        {
            get
            {
                if (drawers.Length <= 1)
                {
                    return 0;
                }
                var p = getPercentage().Clamped(0, 1);
                var bucketSize = 1.0f / (drawers.Length - 1);
                return MoreMath.RoundToInt(p / bucketSize);
            }
        }

        public BucketedIconDrawer(ImmutableArray<IStatusDrawer> drawers, Func<float> getPercentage)
        {
            this.drawers = drawers;
            this.getPercentage = getPercentage;
        }

        public void Draw(CoreDrawers core, IComponentDrawer drawer, Vector3 position, float size)
        {
            drawers[index].Draw(core, drawer, position, size);
        }
    }
}
