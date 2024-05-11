using System;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Enum = System.Enum;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("dischargeMode")]
sealed class DischargeModeProperty : Component, IProperty<CapacitorDischargeMode>, IDischargeModeSetter
{
    private IStatusTracker? statusTracker;
    private IStatusReceipt? status;

    public CapacitorDischargeMode Value { get; private set; } = CapacitorDischargeMode.MinimumCharge;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IStatusTracker>(Owner, Events, t => statusTracker = t);
    }

    public override void Activate()
    {
        base.Activate();
        status = statusTracker?.AddStatus(
            new StatusSpec(StatusType.Neutral, new InteractionSpec(this)),
            StatusAppearance.IconOnly(iconForMode(Value)),
            null);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void SetDischargeMode(CapacitorDischargeMode newMode)
    {
        Value = newMode;
        status?.UpdateAppearance(StatusAppearance.IconOnly(iconForMode(Value)));
    }

    private static ModAwareSpriteId iconForMode(CapacitorDischargeMode mode)
    {
        return mode switch
        {
            CapacitorDischargeMode.MinimumCharge => "battery-25".ToStatusIconSpriteId(),
            CapacitorDischargeMode.FullCharge => "battery-100".ToStatusIconSpriteId(),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    private sealed class InteractionSpec(DischargeModeProperty subject) : IStatusInteractionSpec
    {
        public void Interact(GameRequestDispatcher requestDispatcher)
        {
            var currentIndex = (int) subject.Value;
            var newIndex = (currentIndex + 1) % Enum.GetValues<CapacitorDischargeMode>().Length;
            var newMode = (CapacitorDischargeMode) newIndex;
            requestDispatcher.Request(Elements.SetDischargeMode.Request, subject.Owner, newMode);
        }
    }
}
