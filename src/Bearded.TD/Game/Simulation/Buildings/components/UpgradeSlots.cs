using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Upgrades;
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
    private readonly List<Slot> slots = [];

    private IBuildingUpgradeManager? upgradeManager;

    public IReadOnlyList<IUpgradeSlot> Slots { get; }

    public UpgradeSlots(IParameters parameters) : base(parameters)
    {
        Slots = slots.AsReadOnly();
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IBuildingUpgradeManager>(Owner, Events, um => upgradeManager = um);
    }

    public override void Activate()
    {
        slots.AddRange(Enumerable.Range(0, Parameters.InitialSlots).Select(i => new Slot(this, i)));
        if (!Parameters.AdditionalSlotTriggers.IsDefault)
        {
            triggerSubscriptions.AddRange(
                Parameters.AdditionalSlotTriggers.Select(trigger => trigger.Subscribe(Events, unlockSlot)));
        }
    }

    public override void OnRemoved()
    {
        // TODO: should we undo all upgrades as well?
        foreach (var subscription in triggerSubscriptions)
        {
            subscription.Unsubscribe(Events);
        }
        triggerSubscriptions.Clear();
        base.OnRemoved();
    }

    private void unlockSlot()
    {
        slots.Add(new Slot(this, slots.Count));
    }

    public void FillSlot(IPermanentUpgrade upgrade)
    {
        var slot = Slots.FirstOrDefault(s => !s.Filled);
        if (slot is null)
        {
            throw new InvalidOperationException("Cannot fill a slot if no slots are available");
        }
        slot.Fill(upgrade);
    }

    public override void Update(TimeSpan elapsedTime) { }

    private sealed class Slot(UpgradeSlots slots, int index) : IUpgradeSlot
    {
        public IPermanentUpgrade? Upgrade { get; private set; }

        public void Fill(IPermanentUpgrade upgrade)
        {
            if (Upgrade is not null)
            {
                throw new InvalidOperationException("Cannot override an existing upgrade in a slot");
            }

            if (slots.upgradeManager is null)
            {
                slots.Owner.Game.Meta.Logger.Warning?.Log(
                    $"Attempted to apply {upgrade.Name} to {slots.Owner} but no upgrade manager was present, making " +
                    $"this a no-op. Slot was still filled.");
            }
            slots.upgradeManager?.Upgrade(upgrade);

            Upgrade = upgrade;
        }
    }
}
