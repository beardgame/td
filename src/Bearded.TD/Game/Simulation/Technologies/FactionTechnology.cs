using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Utilities.Collections;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Technologies;

[FactionBehavior("technology")]
sealed class FactionTechnology : FactionBehavior
{
    private readonly HashSet<ITechnologyBlueprint> unlockedTechnologies = new();
    private readonly HashSet<IComponentOwnerBlueprint> unlockedBuildings = new();
    private readonly HashSet<IPermanentUpgrade> unlockedUpgrades = new();

    public bool HasTechnologyToken { get; private set; }

    public IEnumerable<IComponentOwnerBlueprint> UnlockedBuildings => unlockedBuildings.AsReadOnlyEnumerable();

    protected override void Execute() {}

    public bool CanUnlockTechnologyNow(ITechnologyBlueprint technology) =>
        CanUnlockTechnology(technology)
        && canAffordNow();

    public bool CanUnlockTechnology(ITechnologyBlueprint technology) =>
        IsTechnologyLocked(technology)
        && HasAllRequiredTechs(technology);

    public bool IsTechnologyLocked(ITechnologyBlueprint technology) => !unlockedTechnologies.Contains(technology);

    public bool HasAllRequiredTechs(ITechnologyBlueprint technology) =>
        technology.RequiredTechs.All(unlockedTechnologies.Contains);

    private bool canAffordNow() => HasTechnologyToken;

    public void UnlockTechnology(ITechnologyBlueprint technology)
    {
        Argument.Satisfies(() => CanUnlockTechnology(technology));
        consumeTechnologyToken();
        doTechnologyUnlock(technology);
    }

    public void ForceUnlockTechnology(ITechnologyBlueprint technology)
    {
        if (HasTechnologyToken)
        {
            consumeTechnologyToken();
        }
        doTechnologyUnlock(technology);
    }

    private void doTechnologyUnlock(ITechnologyBlueprint technology)
    {
        unlockedTechnologies.Add(technology);
        technology.Unlocks.ForEach(unlock => unlock.Apply(this));
        Events.Send(new TechnologyUnlocked(this, technology));
    }

    public bool IsBuildingUnlocked(IComponentOwnerBlueprint blueprint) => unlockedBuildings.Contains(blueprint);

    public void UnlockBuilding(IComponentOwnerBlueprint blueprint)
    {
        if (unlockedBuildings.Add(blueprint))
        {
            Events.Send(new BuildingTechnologyUnlocked(this, blueprint));
        }
    }

    public bool IsUpgradeUnlocked(IPermanentUpgrade blueprint) => unlockedUpgrades.Contains(blueprint);

    public IEnumerable<IPermanentUpgrade> GetApplicableUpgradesFor(IUpgradable obj) =>
        unlockedUpgrades.Where(obj.CanApplyUpgrade);

    public void UnlockUpgrade(IPermanentUpgrade blueprint)
    {
        if (unlockedUpgrades.Add(blueprint))
        {
            Events.Send(new UpgradeTechnologyUnlocked(this, blueprint));
        }
    }

    private void consumeTechnologyToken()
    {
        State.Satisfies(HasTechnologyToken);
        HasTechnologyToken = false;
        Events.Send(new TechnologyTokenConsumed());
    }

    public void AwardTechnologyToken()
    {
        HasTechnologyToken = true;
        Events.Send(new TechnologyTokenAwarded());
    }
}
