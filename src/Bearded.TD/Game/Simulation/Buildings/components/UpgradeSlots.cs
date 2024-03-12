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

    private readonly List<ITriggerSubscription> triggerSubscriptions = [];

    public int TotalSlotsCount { get; private set; }
    public int FilledSlotsCount { get; private set; }

    public UpgradeSlots(IParameters parameters) : base(parameters) { }

    protected override void OnAdded() { }

    public override void Activate()
    {
        TotalSlotsCount = Parameters.InitialSlots;
        if (!Parameters.AdditionalSlotTriggers.IsDefault)
        {
            triggerSubscriptions.AddRange(
                Parameters.AdditionalSlotTriggers.Select(trigger => trigger.Subscribe(Events, unlockSlot)));
        }
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

    public void FillSlot()
    {
        if (FilledSlotsCount >= TotalSlotsCount)
        {
            throw new InvalidOperationException("Cannot fill a slot when none are available.");
        }

        FilledSlotsCount++;
    }

    public override void Update(TimeSpan elapsedTime) { }
}
