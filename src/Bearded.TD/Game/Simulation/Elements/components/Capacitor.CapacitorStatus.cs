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

        public CapacitorStatus(IStatusTracker statusTracker)
        {
            icons = new SpriteBuckets(
                ImmutableArray.Create(
                    "battery-0".ToStatusIconSpriteId(),
                    "battery-25".ToStatusIconSpriteId(),
                    "battery-50".ToStatusIconSpriteId(),
                    "battery-75".ToStatusIconSpriteId(),
                    "battery-100".ToStatusIconSpriteId()));
            status = statusTracker.AddStatus(
                new StatusSpec(StatusType.Neutral, null),
                statusAppearance(),
                null);
        }

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
}
