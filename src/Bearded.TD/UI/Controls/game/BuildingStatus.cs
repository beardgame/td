using System;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.UI.Controls;

sealed class BuildingStatus : IDisposable
{
    public IReadonlyBinding<bool> ShowExpanded => showExpanded;
    public ReadOnlyObservableCollection<IReadonlyBinding<Status>> Statuses { get; private set; }
    public ReadOnlyObservableCollection<IReadonlyBinding<UpgradeSlot>> Upgrades { get; private set; }
    public IReadonlyBinding<VeterancyStatus> Veterancy => veterancyStatus;

    private readonly IStatusTracker statusTracker;
    private readonly IUpgradeSlots upgradeSlots;
    private readonly IBuildingUpgradeManager upgradeManager;
    private readonly IVeterancy veterancy;

    private readonly Binding<bool> showExpanded = new(false);
    private readonly ObservableCollection<IReadonlyBinding<Status>> statuses;
    private readonly ObservableCollection<IReadonlyBinding<UpgradeSlot>> upgrades;
    private readonly Binding<VeterancyStatus> veterancyStatus = new();

    public BuildingStatus(
        IStatusTracker statusTracker,
        IUpgradeSlots upgradeSlots,
        IBuildingUpgradeManager upgradeManager,
        IVeterancy veterancy)
    {
        this.statusTracker = statusTracker;
        this.upgradeSlots = upgradeSlots;
        this.upgradeManager = upgradeManager;
        this.veterancy = veterancy;

        statuses = new ObservableCollection<IReadonlyBinding<Status>>(statusTracker.Statuses.Select(Binding.Create));
        upgrades = new ObservableCollection<IReadonlyBinding<UpgradeSlot>>(
            upgradeSlots.Slots.Select(UpgradeSlot.FromState).Select(Binding.Create));
        Statuses = new ReadOnlyObservableCollection<IReadonlyBinding<Status>>(statuses);
        Upgrades = new ReadOnlyObservableCollection<IReadonlyBinding<UpgradeSlot>>(upgrades);

        veterancyStatus.SetFromSource(veterancy.GetVeterancyStatus());

        statusTracker.StatusAdded += statusAdded;
        statusTracker.StatusRemoved += statusRemoved;
        upgradeSlots.SlotUnlocked += slotUnlocked;
        upgradeSlots.SlotFilled += slotFilled;
        veterancy.VeterancyStatusChanged += veterancyStatusChanged;
    }

    public void Dispose()
    {
        statusTracker.StatusAdded -= statusAdded;
        statusTracker.StatusRemoved -= statusRemoved;
        upgradeSlots.SlotUnlocked -= slotUnlocked;
        upgradeSlots.SlotFilled -= slotFilled;
        veterancy.VeterancyStatusChanged -= veterancyStatusChanged;
    }

    private void statusAdded(Status status)
    {
        var index = statusTracker.Statuses.IndexOf(status);
        statuses.Insert(index, Binding.Create(status));
    }

    private void statusRemoved(Status status)
    {
        // Not very performant, but the alternative is keeping a status -> index dictionary which is annoying
        // bookkeeping.
        var index = statuses.Indexed().Where(tuple => tuple.Item1.Value == status).Select(tuple => tuple.Item2).First();
        statuses.RemoveAt(index);
    }

    private void slotUnlocked(int index)
    {
        upgrades.Add(Binding.Create(UpgradeSlot.Empty()));
    }

    private void slotFilled(int index, IPermanentUpgrade upgrade)
    {
        upgradeBinding(index).SetFromSource(new UpgradeSlot(upgrade));
    }

    // We are keeping track of `IReadonlyBinding<T>` in the list to not expose the mutable interface, but that means we
    // need to cast here to get the mutable version back.
    private Binding<UpgradeSlot> upgradeBinding(int i) => (Binding<UpgradeSlot>) upgrades[i];

    private void veterancyStatusChanged(VeterancyStatus status)
    {
        veterancyStatus.SetFromSource(status);
    }

    public void PromoteToExpandedView()
    {
        showExpanded.SetFromSource(true);
    }
}
