using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Utilities.Collections;
using static Bearded.TD.Constants.Game.Technology;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Technologies
{
    [FactionBehavior("technology")]
    sealed class FactionTechnology : FactionBehavior<Faction>
    {
        private readonly Dictionary<ITechnologyBlueprint, long> unlockedTechnologies = new();
        private readonly HashSet<IBuildingBlueprint> unlockedBuildings = new();
        private readonly HashSet<IUpgradeBlueprint> unlockedUpgrades = new();
        private readonly List<ITechnologyBlueprint> queuedTechnologies = new();

        public long TechPoints { get; private set; }

        private double techCostMultiplier = 1;

        public IEnumerable<IBuildingBlueprint> UnlockedBuildings => unlockedBuildings.AsReadOnlyEnumerable();

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

            if (queuedTechnologies.Count == 0 && canAffordNow(technology))
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

        public bool IsTechnologyLocked(ITechnologyBlueprint technology) => !unlockedTechnologies.ContainsKey(technology);

        public bool CanUnlockTechnology(ITechnologyBlueprint technology) =>
            IsTechnologyLocked(technology)
            && canAffordNow(technology)
            && HasAllRequiredTechs(technology);

        private bool canAffordNow(ITechnologyBlueprint technology) => TechPoints >= costIfUnlockedNow(technology);

        private long costIfUnlockedNow(ITechnologyBlueprint technology) =>
            costAtPositionInQueue(technology, 0);

        private long costAtPositionInQueue(ITechnologyBlueprint technology, int position)
        {
            var multiplier = techCostMultiplier * Math.Pow(TechCostMultiplicationFactor, position);
            return (long) (technology.Cost * multiplier);
        }

        public long ExpectedCost(ITechnologyBlueprint technology)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (techCostMultiplier == 1)
            {
                return technology.Cost;
            }

            if (unlockedTechnologies.ContainsKey(technology))
            {
                return unlockedTechnologies[technology];
            }

            if (IsTechnologyQueued(technology))
            {
                return costAtPositionInQueue(technology, QueuePositionFor(technology));
            }

            return costAtPositionInQueue(
                technology, queuedTechnologies.Count + countLockedAndNotQueuedDependencies(technology));
        }

        private int countLockedAndNotQueuedDependencies(ITechnologyBlueprint technology)
        {
            var seen = new HashSet<ITechnologyBlueprint>();
            return countInternal(technology) - 1; // Since we include the current technology in the count.

            int countInternal(ITechnologyBlueprint tech)
            {
                seen.Add(tech);
                return 1 + tech.RequiredTechs
                    .Where(dependency => !seen.Contains(dependency)
                         && IsTechnologyLocked(dependency)
                         && !IsTechnologyQueued(dependency))
                    .Sum(countInternal);
            }
        }

        public bool HasAllRequiredTechs(ITechnologyBlueprint technology) =>
            technology.RequiredTechs.All(unlockedTechnologies.ContainsKey);

        private void tryUnlockQueuedTechnologies()
        {
            while (queuedTechnologies.Count > 0 && canAffordNow(queuedTechnologies[0]))
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

            var cost = costIfUnlockedNow(technology);
            TechPoints -= cost;
            techCostMultiplier *= TechCostMultiplicationFactor;
            unlockedTechnologies.Add(technology, cost);
            technology.Unlocks.ForEach(unlock => unlock.Apply(this));
            Events.Send(new TechnologyUnlocked(this, technology));
        }

        public bool IsBuildingUnlocked(IBuildingBlueprint blueprint) => unlockedBuildings.Contains(blueprint);

        public void UnlockBuilding(IBuildingBlueprint blueprint)
        {
            if (unlockedBuildings.Add(blueprint))
            {
                Events.Send(new BuildingTechnologyUnlocked(this, blueprint));
            }
        }

        public bool IsUpgradeUnlocked(IUpgradeBlueprint blueprint) => unlockedUpgrades.Contains(blueprint);

        public IEnumerable<IUpgradeBlueprint> GetApplicableUpgradesFor(Building building) =>
            unlockedUpgrades.Where(building.CanApplyUpgrade);

        public void UnlockUpgrade(IUpgradeBlueprint blueprint)
        {
            if (unlockedUpgrades.Add(blueprint))
            {
                Events.Send(new UpgradeTechnologyUnlocked(this, blueprint));
            }
        }

        public void AddTechPoints(long number)
        {
            TechPoints += number;
            tryUnlockQueuedTechnologies();
        }
    }
}
