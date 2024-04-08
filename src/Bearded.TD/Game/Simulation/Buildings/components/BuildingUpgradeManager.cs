using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed partial class BuildingUpgradeManager
    : Component,
        IBuildingUpgradeManager,
        IListener<ComponentAdded>
{
    private ObjectAttributes attributes = ObjectAttributes.Default;
    private IFactionProvider? factionProvider;
    private readonly List<IPermanentUpgrade> appliedUpgrades = [];
    public IReadOnlyCollection<IPermanentUpgrade> AppliedUpgrades { get; }

    public IEnumerable<IPermanentUpgrade> ApplicableUpgrades
    {
        get
        {
            if (factionProvider == null)
            {
                return Enumerable.Empty<IPermanentUpgrade>();
            }
            return factionProvider.Faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var technology)
                ? technology.GetApplicableUpgradesFor(this)
                : Enumerable.Empty<IPermanentUpgrade>();
        }
    }

    public event GenericEventHandler<IPermanentUpgrade>? UpgradeCompleted;

    public BuildingUpgradeManager()
    {
        AppliedUpgrades = appliedUpgrades.AsReadOnly();
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
        ReportAggregator.Register(Events, new UpgradeReport(this));
        ComponentDependencies.Depend<ObjectAttributes>(Owner, Events, attr => attributes = attr);
        ComponentDependencies.Depend<IFactionProvider>(Owner, Events, provider => factionProvider = provider);
    }

    public bool CanBeUpgradedBy(Faction faction) =>
        factionProvider?.Faction.OwnedBuildingsCanBeUpgradedBy(faction) ?? false;

    public bool CanApplyUpgrade(IPermanentUpgrade upgrade)
    {
        return !appliedUpgrades.Contains(upgrade) && Owner.CanApplyUpgrade(upgrade);
    }

    public void Upgrade(IPermanentUpgrade upgrade)
    {
        applyUpgrade(upgrade);
    }

    private void applyUpgrade(IPermanentUpgrade upgrade)
    {
        Owner.ApplyUpgrade(upgrade);

        appliedUpgrades.Add(upgrade);

        UpgradeCompleted?.Invoke(upgrade);
        Owner.Game.Meta.Events.Send(
            new BuildingUpgradeFinished(attributes.Name, Owner, upgrade));
    }

    public override void Update(TimeSpan elapsedTime) {}

    public void HandleEvent(ComponentAdded @event)
    {
        // Replay all past upgrades on this new component
        foreach (var u in appliedUpgrades)
        {
            @event.Component.ApplyUpgrade(u);
        }
    }
}

interface IBuildingUpgradeManager : IUpgradable
{
    IReadOnlyCollection<IPermanentUpgrade> AppliedUpgrades { get; }
    IEnumerable<IPermanentUpgrade> ApplicableUpgrades { get; }

    event GenericEventHandler<IPermanentUpgrade> UpgradeCompleted;

    bool CanBeUpgradedBy(Faction faction);
    void Upgrade(IPermanentUpgrade upgrade);
}
