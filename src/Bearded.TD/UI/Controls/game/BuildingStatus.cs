using System;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Audio;
using Bearded.TD.Content;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Statistics;
using Bearded.TD.Game.Simulation.Statistics.Data;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.UI.Controls;

sealed class BuildingStatus
    : IDisposable,
        IListener<UpgradeTechnologyUnlocked>,
        IListener<ResourcesProvided<Scrap>>,
        IListener<ResourcesConsumed<Scrap>>
{
    private readonly GameRequestDispatcher requestDispatcher;
    private readonly ContentManager contentManager;
    private readonly ISoundScape soundScape;
    private readonly GameObject building;
    private readonly IGameStatistics gameStatistics;
    private readonly IObjectAttributes attributes;
    private readonly GlobalGameEvents events;
    private readonly FactionResources? resources;
    private readonly IStatusTracker statusTracker;
    private readonly IUpgradeSlots? upgradeSlots;
    private readonly IVeterancy? veterancy;
    private ITowerStatisticObserver towerStatisticObserver;

    private readonly Binding<bool> showExpanded = new(false);
    private readonly ObservableCollection<ObservableStatus> statuses;
    private readonly ObservableCollection<IReadonlyBinding<UpgradeSlot>> upgrades;
    private readonly ObservableCollection<IPermanentUpgrade> availableUpgrades;
    private readonly Binding<VeterancyStatus> veterancyStatus = new();
    private readonly Binding<AccumulatedDamage> damageThisWave = new(AccumulatedDamage.Zero);
    private readonly Binding<int?> activeUpgradeSlot = new();
    private readonly Binding<bool> showUpgradeSelect = new();
    private readonly Binding<Resource<Scrap>> currentResources = new();

    public bool ShowVeterancy => veterancy is not null;
    public bool ShowUpgrades => upgradeSlots is not null;
    public bool ShowDeletion { get; }

    public IReadonlyBinding<bool> ShowExpanded => showExpanded;
    public string BuildingName => attributes.Name;
    public ModAwareSpriteId? Icon => attributes.Icon;
    public ReadOnlyObservableCollection<ObservableStatus> Statuses { get; }
    public ReadOnlyObservableCollection<IReadonlyBinding<UpgradeSlot>> Upgrades { get; }
    public ReadOnlyObservableCollection<IPermanentUpgrade> AvailableUpgrades { get; }
    public IReadonlyBinding<VeterancyStatus> Veterancy => veterancyStatus;
    public IReadonlyBinding<AccumulatedDamage> DamageThisWave => damageThisWave;
    public IReadonlyBinding<int?> ActiveUpgradeSlot => activeUpgradeSlot;
    public IReadonlyBinding<bool> ShowUpgradeSelect => showUpgradeSelect;
    public IReadonlyBinding<Resource<Scrap>> CurrentResources => currentResources;

    public BuildingStatus(
        GameRequestDispatcher requestDispatcher,
        ContentManager contentManager,
        ISoundScape soundScape,
        GameObject building,
        IStatusTracker statusTracker,
        IUpgradeSlots? upgradeSlots,
        IVeterancy? veterancy)
    {
        this.requestDispatcher = requestDispatcher;
        this.building = building;
        gameStatistics = building.Game.Statistics;
        attributes = this.building.AttributesOrDefault();
        events = building.Game.Meta.Events;
        building.FindFaction().TryGetBehaviorIncludingAncestors(out resources);
        this.statusTracker = statusTracker;
        this.upgradeSlots = upgradeSlots;
        this.veterancy = veterancy;
        towerStatisticObserver = observeStatistics();
        this.soundScape = soundScape;
        this.contentManager = contentManager;

        ShowDeletion = building.CanBeDeleted();

        statuses = new ObservableCollection<ObservableStatus>(
            statusTracker.Statuses.Select(s => new ObservableStatus(s)));
        upgrades = new ObservableCollection<IReadonlyBinding<UpgradeSlot>>(
            upgradeSlots?.Slots.Select(UpgradeSlot.FromState).Select(Binding.Create) ??
            Enumerable.Empty<IReadonlyBinding<UpgradeSlot>>());
        availableUpgrades = new ObservableCollection<IPermanentUpgrade>(
            upgradeSlots?.AvailableUpgrades.OrderBy(u => u.Name) ?? Enumerable.Empty<IPermanentUpgrade>());
        Statuses = new ReadOnlyObservableCollection<ObservableStatus>(statuses);
        Upgrades = new ReadOnlyObservableCollection<IReadonlyBinding<UpgradeSlot>>(upgrades);
        AvailableUpgrades = new ReadOnlyObservableCollection<IPermanentUpgrade>(availableUpgrades);

        veterancyStatus.SetFromSource(veterancy?.GetVeterancyStatus() ?? VeterancyStatus.Initial);
        activeUpgradeSlot.SetFromSource(findActiveUpgradeSlot());
        currentResources.SetFromSource(resources?.GetCurrent<Scrap>() ?? Resource<Scrap>.Zero);
        onTowerStatisticsUpdated();

        statusTracker.StatusAdded += statusAdded;
        statusTracker.StatusRemoved += statusRemoved;
        if (upgradeSlots is not null)
        {
            upgradeSlots.SlotUnlocked += slotUnlocked;
            upgradeSlots.SlotFilled += slotFilled;
        }
        if (veterancy is not null)
        {
            veterancy.VeterancyStatusChanged += veterancyStatusChanged;
        }

        events.Subscribe<UpgradeTechnologyUnlocked>(this);
        events.Subscribe<ResourcesProvided<Scrap>>(this);
        events.Subscribe<ResourcesConsumed<Scrap>>(this);
    }

    private int? findActiveUpgradeSlot()
    {
        var foundSlot = Enumerable.Range(0, upgradeSlots?.Slots.Count ?? 0)
            .FirstOrDefault(i => !upgradeSlots!.Slots[i].Filled, -1);
        return foundSlot >= 0 ? foundSlot : null;
    }

    private ITowerStatisticObserver observeStatistics()
    {
        var observer = gameStatistics.ObserveTower(building);
        observer.StatisticsUpdated += onTowerStatisticsUpdated;
        observer.Disposed += onTowerStatisticsDisposed;
        return observer;
    }

    private void onTowerStatisticsUpdated()
    {
        damageThisWave.SetFromSource(towerStatisticObserver.TotalDamage);
    }

    private void onTowerStatisticsDisposed()
    {
        towerStatisticObserver = observeStatistics();
        damageThisWave.SetFromSource(AccumulatedDamage.Zero);
    }

    public void Dispose()
    {
        foreach (var s in statuses)
        {
            s.Dispose();
        }

        statusTracker.StatusAdded -= statusAdded;
        statusTracker.StatusRemoved -= statusRemoved;
        if (upgradeSlots is not null)
        {
            upgradeSlots.SlotUnlocked -= slotUnlocked;
            upgradeSlots.SlotFilled -= slotFilled;
        }
        if (veterancy is not null)
        {
            veterancy.VeterancyStatusChanged -= veterancyStatusChanged;
        }
        towerStatisticObserver.StopObserving();

        events.Unsubscribe<UpgradeTechnologyUnlocked>(this);
        events.Unsubscribe<ResourcesProvided<Scrap>>(this);
        events.Unsubscribe<ResourcesConsumed<Scrap>>(this);
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
        upgradeSlots?.AvailableUpgrades.OrderBy(u => u.Name).ForEach(availableUpgrades.Add);
    }

    private void veterancyStatusChanged(VeterancyStatus status)
    {
        veterancyStatus.SetFromSource(status);
    }

    public void HandleEvent(UpgradeTechnologyUnlocked @event)
    {
        // We could consider filtering by faction, but realistically right now we only have one faction.
        updateAvailableUpgrades();
    }

    public void HandleEvent(ResourcesProvided<Scrap> @event)
    {
        if (@event.Resources == resources)
        {
            currentResources.SetFromSource(resources.GetCurrent<Scrap>());
        }
    }

    public void HandleEvent(ResourcesConsumed<Scrap> @event)
    {
        if (@event.Resources == resources)
        {
            currentResources.SetFromSource(resources.GetCurrent<Scrap>());
        }
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

        var sound = upgrade.Element.GetUpgradeSound(contentManager);
        soundScape.PlayGlobalSound(sound);
    }

    public void DeleteBuilding()
    {
        requestDispatcher.Request(Game.Simulation.Buildings.DeleteBuilding.Request, building);
    }
}
