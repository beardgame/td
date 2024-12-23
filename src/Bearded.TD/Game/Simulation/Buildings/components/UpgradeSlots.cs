using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Technologies;
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

    private IFactionProvider? factionProvider;

    private readonly List<ITriggerSubscription> triggerSubscriptions = [];
    private readonly List<Slot> slots = [];

    public IReadOnlyList<IUpgradeSlot> Slots { get; }

    public IReadOnlyList<IPermanentUpgrade> AvailableUpgrades
    {
        get
        {
            if (factionProvider == null) return [];
            return factionProvider.Faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var technology)
                ? [..technology.GetApplicableUpgradesFor(this)]
                : [];
        }
    }

    public event SlotEventHandler? SlotUnlocked;
    public event SlotFilledEventHandler? SlotFilled;

    public UpgradeSlots(IParameters parameters) : base(parameters)
    {
        Slots = slots.AsReadOnly();
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IFactionProvider>(Owner, Events, p => factionProvider = p);
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
        var index = slots.Count;
        slots.Add(new Slot(this, index));
        SlotUnlocked?.Invoke(index);
    }

    public void FillSlot(IPermanentUpgrade upgrade)
    {
        var slot = slots.FirstOrDefault(s => !((IUpgradeSlot) s).Filled);
        if (slot is null)
        {
            throw new InvalidOperationException("Cannot fill a slot if no slots are available");
        }
        slot.Fill(upgrade);
        SlotFilled?.Invoke(slot.Index, upgrade);
    }

    public bool CanApplyUpgrade(IPermanentUpgrade upgrade)
    {
        return slots.Select(s => s.Upgrade).All(u => u != upgrade) && Owner.CanApplyUpgrade(upgrade);
    }

    public override void Update(TimeSpan elapsedTime) { }

    private sealed class Slot(UpgradeSlots slots, int index) : IUpgradeSlot
    {
        public int Index { get; } = index;
        public IPermanentUpgrade? Upgrade { get; private set; }

        public void Fill(IPermanentUpgrade upgrade)
        {
            if (Upgrade is not null)
            {
                throw new InvalidOperationException("Cannot override an existing upgrade in a slot");
            }

            slots.Owner.ApplyUpgrade(upgrade);
            Upgrade = upgrade;
        }
    }
}
