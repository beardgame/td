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
    private readonly HashSet<TechnologyBranchTier> unlockedTiers = [];
    private readonly HashSet<ITechnologyBlueprint> unlockedTechnologies = [];
    private readonly HashSet<IGameObjectBlueprint> unlockedBuildings = [];
    private readonly HashSet<IPermanentUpgrade> unlockedUpgrades = [];

    public bool HasTechnologyToken { get; private set; }

    public IEnumerable<IGameObjectBlueprint> UnlockedBuildings => unlockedBuildings.AsReadOnlyEnumerable();

    protected override void Execute() {}

    public bool CanUnlockTechnologyNow(ITechnologyBlueprint technology) =>
        CanUnlockTechnology(technology)
        && canAffordNow();

    public bool CanUnlockTechnology(ITechnologyBlueprint technology) =>
        IsTechnologyLocked(technology)
        && hasRequiredTier(technology)
        && HasAllRequiredTechs(technology);

    public bool IsTechnologyLocked(ITechnologyBlueprint technology) => !unlockedTechnologies.Contains(technology);

    private bool hasRequiredTier(ITechnologyBlueprint technology) =>
        technology.Tier == TechnologyTier.Free || IsTierUnlocked(technology.Branch, technology.Tier);

    public bool IsTierUnlocked(TechnologyBranch branch, TechnologyTier tier) =>
        IsTierUnlocked(new TechnologyBranchTier(branch, tier));

    public bool IsTierUnlocked(TechnologyBranchTier branchTier) => unlockedTiers.Contains(branchTier);

    public bool HasAllRequiredTechs(ITechnologyBlueprint technology) =>
        technology.RequiredTechs.All(unlockedTechnologies.Contains);

    private bool canAffordNow() => HasTechnologyToken;

    public void UnlockTier(TechnologyBranch branch, TechnologyTier tier)
    {
        UnlockTier(new TechnologyBranchTier(branch, tier));
    }

    public void UnlockTier(TechnologyBranchTier branchTier)
    {
        if (unlockedTiers.Add(branchTier))
        {
            Events.Send(new TierUnlocked(this, branchTier));
        }
    }

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

    public bool IsBuildingUnlocked(IGameObjectBlueprint blueprint) => unlockedBuildings.Contains(blueprint);

    public void UnlockBuilding(IGameObjectBlueprint blueprint)
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
        Events.Send(new TechnologyTokenConsumed(this));
    }

    public void AwardTechnologyToken()
    {
        HasTechnologyToken = true;
        Events.Send(new TechnologyTokenAwarded(this));
    }
}
