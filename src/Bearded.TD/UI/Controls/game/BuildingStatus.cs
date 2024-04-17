using System;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.UI.Controls;

sealed class BuildingStatus : IDisposable
{
    private readonly GameRequestDispatcher requestDispatcher;
    private readonly GameObject building;
    private readonly IStatusTracker statusTracker;
    private readonly IUpgradeSlots upgradeSlots;
    private readonly IVeterancy veterancy;

    private readonly Binding<bool> showExpanded = new(false);
    private readonly ObservableCollection<ObservableStatus> statuses;
    private readonly ObservableCollection<IReadonlyBinding<UpgradeSlot>> upgrades;
    private readonly ObservableCollection<IPermanentUpgrade> availableUpgrades;
    private readonly Binding<VeterancyStatus> veterancyStatus = new();
    private readonly Binding<int?> activeUpgradeSlot = new();
    private readonly Binding<bool> showUpgradeSelect = new();

    public IReadonlyBinding<bool> ShowExpanded => showExpanded;
    public ReadOnlyObservableCollection<ObservableStatus> Statuses { get; }
    public ReadOnlyObservableCollection<IReadonlyBinding<UpgradeSlot>> Upgrades { get; }
    public ReadOnlyObservableCollection<IPermanentUpgrade> AvailableUpgrades { get; }
    public IReadonlyBinding<VeterancyStatus> Veterancy => veterancyStatus;
    public IReadonlyBinding<int?> ActiveUpgradeSlot => activeUpgradeSlot;
    public IReadonlyBinding<bool> ShowUpgradeSelect => showUpgradeSelect;

    public BuildingStatus(
        GameRequestDispatcher requestDispatcher,
        GameObject building,
        IStatusTracker statusTracker,
        IUpgradeSlots upgradeSlots,
        IVeterancy veterancy)
    {
        this.requestDispatcher = requestDispatcher;
        this.building = building;
        this.statusTracker = statusTracker;
        this.upgradeSlots = upgradeSlots;
        this.veterancy = veterancy;

        statuses = new ObservableCollection<ObservableStatus>(
            statusTracker.Statuses.Select(s => new ObservableStatus(s)));
        upgrades = new ObservableCollection<IReadonlyBinding<UpgradeSlot>>(
            upgradeSlots.Slots.Select(UpgradeSlot.FromState).Select(Binding.Create));
        availableUpgrades = new ObservableCollection<IPermanentUpgrade>(upgradeSlots.AvailableUpgrades);
        Statuses = new ReadOnlyObservableCollection<ObservableStatus>(statuses);
        Upgrades = new ReadOnlyObservableCollection<IReadonlyBinding<UpgradeSlot>>(upgrades);
        AvailableUpgrades = new ReadOnlyObservableCollection<IPermanentUpgrade>(availableUpgrades);

        veterancyStatus.SetFromSource(veterancy.GetVeterancyStatus());
        activeUpgradeSlot.SetFromSource(findActiveUpgradeSlot());

        statusTracker.StatusAdded += statusAdded;
        statusTracker.StatusRemoved += statusRemoved;
        upgradeSlots.SlotUnlocked += slotUnlocked;
        upgradeSlots.SlotFilled += slotFilled;
        veterancy.VeterancyStatusChanged += veterancyStatusChanged;
    }

    private int? findActiveUpgradeSlot()
    {
        var foundSlot = Enumerable.Range(0, upgradeSlots.Slots.Count)
            .FirstOrDefault(i => !upgradeSlots.Slots[i].Filled, -1);
        return foundSlot >= 0 ? foundSlot : null;
    }

    public void Dispose()
    {
        foreach (var s in statuses)
        {
            s.Dispose();
        }

        statusTracker.StatusAdded -= statusAdded;
        statusTracker.StatusRemoved -= statusRemoved;
        upgradeSlots.SlotUnlocked -= slotUnlocked;
        upgradeSlots.SlotFilled -= slotFilled;
        veterancy.VeterancyStatusChanged -= veterancyStatusChanged;
    }

    private void statusAdded(Status status)
    {
        var index = statusTracker.Statuses.IndexOf(status);
        statuses.Insert(index, new ObservableStatus(status));
    }

    private void statusRemoved(Status status)
    {
        // Not very performant, but the alternative is a lot of bookkeeping.
        var (observableStatus, index) = statuses.Indexed().First(tuple => tuple.Item1.Observes(status));
        observableStatus.Dispose();
        statuses.RemoveAt(index);
    }

    private void slotUnlocked(int index)
    {
        upgrades.Add(Binding.Create(UpgradeSlot.Empty(index)));
        if (activeUpgradeSlot.Value is null)
        {
            activeUpgradeSlot.SetFromSource(index);
        }
    }

    private void slotFilled(int index, IPermanentUpgrade upgrade)
    {
        State.Satisfies(index == activeUpgradeSlot.Value);
        upgradeBinding(index).SetFromSource(new UpgradeSlot(index, upgrade));
        incrementActiveUpgradeSlot();
        updateAvailableUpgrades();
    }

    // We are keeping track of `IReadonlyBinding<T>` in the list to not expose the mutable interface, but that means we
    // need to cast here to get the mutable version back.
    private Binding<UpgradeSlot> upgradeBinding(int i) => (Binding<UpgradeSlot>) upgrades[i];

    private void incrementActiveUpgradeSlot()
    {
        var newlyActiveSlot = activeUpgradeSlot.Value + 1;
        if (newlyActiveSlot >= upgrades.Count)
        {
            activeUpgradeSlot.SetFromSource(null);
            showUpgradeSelect.SetFromSource(false);
        }
        else
        {
            activeUpgradeSlot.SetFromSource(newlyActiveSlot);
        }
    }

    private void updateAvailableUpgrades()
    {
        // Not the most efficient, but we can always implement smarter diffing later (and that may end up being slower).
        availableUpgrades.Clear();
        upgradeSlots.AvailableUpgrades.ForEach(availableUpgrades.Add);
    }

    private void veterancyStatusChanged(VeterancyStatus status)
    {
        veterancyStatus.SetFromSource(status);
    }

    public void PromoteToExpandedView()
    {
        showExpanded.SetFromSource(true);
    }

    public void ToggleUpgradeSelect()
    {
        if (!showExpanded.Value) return;
        showUpgradeSelect.ToggleFromSource();
    }

    public void ApplyUpgrade(IPermanentUpgrade upgrade)
    {
        requestDispatcher.Request(UpgradeBuilding.Request, building, upgrade);
    }

    public void DeleteBuilding()
    {
        requestDispatcher.Request(Game.Simulation.Buildings.DeleteBuilding.Request, building);
    }
}
