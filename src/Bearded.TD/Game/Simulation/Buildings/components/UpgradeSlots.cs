using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("upgradeSlots")]
sealed class UpgradeSlots : Component<UpgradeSlots.IParameters>, IUpgradeSlots
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        int InitialSlots { get; }
        ImmutableArray<ITrigger> AdditionalSlotTriggers { get; }
    }

    private readonly List<ITriggerSubscription> triggerSubscriptions = new();

    public int TotalSlotsCount { get; private set; }
    public int FilledSlotsCount { get; private set; }
    public int ReservedSlotsCount { get; private set; }

    public UpgradeSlots(IParameters parameters) : base(parameters) { }

    protected override void OnAdded() { }

    public override void Activate()
    {
        TotalSlotsCount = Parameters.InitialSlots;
        triggerSubscriptions.AddRange(
            Parameters.AdditionalSlotTriggers.Select(trigger => trigger.Subscribe(Events, unlockSlot)));
    }

    public override void OnRemoved()
    {
        foreach (var subscription in triggerSubscriptions)
        {
            subscription.Unsubscribe(Events);
        }
        triggerSubscriptions.Clear();
        base.OnRemoved();
    }

    private void unlockSlot()
    {
        TotalSlotsCount++;
    }

    public IUpgradeSlotReservation ReserveSlot()
    {
        if (FilledSlotsCount + ReservedSlotsCount >= TotalSlotsCount)
        {
            throw new InvalidOperationException("Cannot reserve a slot when none are available.");
        }

        ReservedSlotsCount++;
        return new SlotReservation(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    private sealed class SlotReservation : IUpgradeSlotReservation
    {
        private readonly UpgradeSlots slots;
        private bool filled;

        public SlotReservation(UpgradeSlots slots)
        {
            this.slots = slots;
        }

        public void Fill()
        {
            // fills are idempotent
            if (filled) return;
            slots.ReservedSlotsCount--;
            slots.FilledSlotsCount++;
            filled = true;
        }
    }
}
