using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Utilities.Collections;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Technologies;

[FactionBehavior("technology")]
sealed class FactionTechnology : FactionBehavior
{
    private readonly HashSet<ITechnologyBlueprint> unlockedTechnologies = new();
    private readonly HashSet<IComponentOwnerBlueprint> unlockedBuildings = new();
    private readonly HashSet<IUpgradeBlueprint> unlockedUpgrades = new();
    private readonly List<ITechnologyBlueprint> queuedTechnologies = new();

    public long TechPoints { get; private set; }
    public bool HasTechnologyToken { get; private set; }

    public IEnumerable<IComponentOwnerBlueprint> UnlockedBuildings => unlockedBuildings.AsReadOnlyEnumerable();

    protected override void Execute() {}

    public bool IsTechnologyQueued(ITechnologyBlueprint technology) => queuedTechnologies.Contains(technology);

    public int QueuePositionFor(ITechnologyBlueprint technologyBlueprint)
    {
        Argument.Satisfies(() => IsTechnologyQueued(technologyBlueprint));
        return queuedTechnologies.FindIndex(t => t == technologyBlueprint) + 1;
    }

    public bool CanQueueTechnology(ITechnologyBlueprint technology) => IsTechnologyLocked(technology);

    public void ReplaceTechnologyQueue(ITechnologyBlueprint technology)
    {
        Argument.Satisfies(() => IsTechnologyLocked(technology));
        ClearTechnologyQueue();
        queueTechnologyAndMissingDependencies(technology);
    }

    public void AddToTechnologyQueue(ITechnologyBlueprint technology)
    {
        Argument.Satisfies(() => IsTechnologyLocked(technology));
        Argument.Satisfies(() => !IsTechnologyQueued(technology));
        queueTechnologyAndMissingDependencies(technology);
    }

    private void queueTechnologyAndMissingDependencies(ITechnologyBlueprint technology)
    {
        if (!IsTechnologyLocked(technology) || IsTechnologyQueued(technology))
        {
            return;
        }

        foreach (var dependency in technology.RequiredTechs)
        {
            queueTechnologyAndMissingDependencies(dependency);
        }

        if (queuedTechnologies.Count == 0 && canAffordNow())
        {
            unlockTechnology(technology);
        }
        else
        {
            queueTechnology(technology);
        }
    }

    private void queueTechnology(ITechnologyBlueprint technology)
    {
        Argument.Satisfies(() => CanQueueTechnology(technology));

        queuedTechnologies.Add(technology);
        Events.Send(new TechnologyQueued(this, technology));
    }

    public void ClearTechnologyQueue()
    {
        while (queuedTechnologies.Count > 0)
        {
            var i = queuedTechnologies.Count - 1;
            Events.Send(new TechnologyDequeued(this, queuedTechnologies[i]));
            queuedTechnologies.RemoveAt(i);
        }
    }

    public bool IsTechnologyLocked(ITechnologyBlueprint technology) => !unlockedTechnologies.Contains(technology);

    public bool CanUnlockTechnology(ITechnologyBlueprint technology) =>
        IsTechnologyLocked(technology)
        && canAffordNow()
        && HasAllRequiredTechs(technology);

    private bool canAffordNow() => HasTechnologyToken || TechPoints >= 1;

    public bool HasAllRequiredTechs(ITechnologyBlueprint technology) =>
        technology.RequiredTechs.All(unlockedTechnologies.Contains);

    private void tryUnlockQueuedTechnologies()
    {
        while (queuedTechnologies.Count > 0 && canAffordNow())
        {
            unlockFirstQueuedTechnology();
        }
    }

    private void unlockFirstQueuedTechnology()
    {
        var technology = queuedTechnologies[0];

        Argument.Satisfies(() => CanUnlockTechnology(technology));

        queuedTechnologies.RemoveAt(0);
        unlockTechnology(technology);
    }

    private void unlockTechnology(ITechnologyBlueprint technology)
    {
        Argument.Satisfies(() => !IsTechnologyQueued(technology));

        if (HasTechnologyToken)
        {
            consumeTechnologyToken();
        }
        else
        {
            TechPoints--;
        }

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

    public bool IsUpgradeUnlocked(IUpgradeBlueprint blueprint) => unlockedUpgrades.Contains(blueprint);

    public IEnumerable<IUpgradeBlueprint> GetApplicableUpgradesFor(IUpgradable obj) =>
        unlockedUpgrades.Where(obj.CanApplyUpgrade);

    public void UnlockUpgrade(IUpgradeBlueprint blueprint)
    {
        if (unlockedUpgrades.Add(blueprint))
        {
            Events.Send(new UpgradeTechnologyUnlocked(this, blueprint));
        }
    }

    public void consumeTechnologyToken()
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

    public void AddTechPoints(long number)
    {
        TechPoints += number;
        tryUnlockQueuedTechnologies();
    }
}
