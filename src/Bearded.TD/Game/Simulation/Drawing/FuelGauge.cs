using System;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing;

[Component("fuelGauge")]
sealed class FuelGauge : Component
{
    private static readonly ModAwareSpriteId iconSpriteId = "fuel-tank".ToStatusIconSpriteId();

    private IFuelTank? tank;
    private float displayLevel;
    private IBuildingStateProvider? building;
    private IStatusTracker? statusDisplay;
    private IStatusReceipt? status;

    private bool isVisible => building?.State.IsCompleted != false;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IFuelTank>(Owner, Events, t => tank = t);
        ComponentDependencies.Depend<IBuildingStateProvider>(Owner, Events, b => building = b);
        ComponentDependencies.Depend<IStatusTracker>(Owner, Events, d => statusDisplay = d);
    }

    public override void Activate() { }

    public override void Update(TimeSpan elapsedTime)
    {
        if (!isVisible || tank is not { FilledPercentage: var level })
        {
            return;
        }

        if (statusDisplay is not null && status is null)
        {
            status = statusDisplay.AddStatus(
                new StatusSpec(StatusType.Neutral, null), StatusAppearance.IconOnly(iconSpriteId), null);
        }

        displayLevel += (level - displayLevel) * (1 - MathF.Pow(0.1f, (float)elapsedTime.NumericValue));
        status?.UpdateAppearance(StatusAppearance.IconAndProgress(iconSpriteId, displayLevel));
    }
}
