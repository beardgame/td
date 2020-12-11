using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using static Bearded.TD.Constants.Game.Technology;

namespace Bearded.TD.Game.Simulation.Technologies
{
    sealed class TechnologyManager : IListener<EnemyKilled>
    {
        private readonly GlobalGameEvents events;

        private readonly Dictionary<ITechnologyBlueprint, long> unlockedTechnologies =
            new Dictionary<ITechnologyBlueprint, long>();
        private readonly HashSet<IBuildingBlueprint> unlockedBuildings = new HashSet<IBuildingBlueprint>();
        private readonly HashSet<IUpgradeBlueprint> unlockedUpgrades = new HashSet<IUpgradeBlueprint>();
        private readonly List<ITechnologyBlueprint> queuedTechnologies = new List<ITechnologyBlueprint>();

        public long TechPoints { get; private set; }

        private double techCostMultiplier = 1;

        public IEnumerable<IBuildingBlueprint> UnlockedBuildings => unlockedBuildings.AsReadOnlyEnumerable();

        public TechnologyManager(GlobalGameEvents events)
        {
            this.events = events;

            events.Subscribe(this);
        }

        public bool IsTechnologyQueued(ITechnologyBlueprint technology) => queuedTechnologies.Contains(technology);

        public int QueuePositionFor(ITechnologyBlueprint technologyBlueprint)
        {
            DebugAssert.Argument.Satisfies(() => IsTechnologyQueued(technologyBlueprint));
            return queuedTechnologies.FindIndex(t => t == technologyBlueprint) + 1;
        }

        public bool CanQueueTechnology(ITechnologyBlueprint technology) => IsTechnologyLocked(technology);

        public void ReplaceTechnologyQueue(ITechnologyBlueprint technology)
        {
            DebugAssert.Argument.Satisfies(() => IsTechnologyLocked(technology));
            ClearTechnologyQueue();
            queueTechnologyAndMissingDependencies(technology);
        }

        public void AddToTechnologyQueue(ITechnologyBlueprint technology)
        {
            DebugAssert.Argument.Satisfies(() => IsTechnologyLocked(technology));
            DebugAssert.Argument.Satisfies(() => !IsTechnologyQueued(technology));
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
            DebugAssert.Argument.Satisfies(() => CanQueueTechnology(technology));

            queuedTechnologies.Add(technology);
            events.Send(new TechnologyQueued(technology));
        }

        public void ClearTechnologyQueue()
        {
            while (queuedTechnologies.Count > 0)
            {
                var i = queuedTechnologies.Count - 1;
                events.Send(new TechnologyDequeued(queuedTechnologies[i]));
                queuedTechnologies.RemoveAt(i);
            }
        }

        public bool IsTechnologyLocked(ITechnologyBlueprint technology) => !unlockedTechnologies.ContainsKey(technology);

        public bool CanUnlockTechnology(ITechnologyBlueprint technology) =>
            IsTechnologyLocked(technology)
            && canAffordNow(technology)
            && HasAllRequiredTechs(technology);

        private bool canAffordNow(ITechnologyBlueprint technology) => TechPoints >= CostIfUnlockedNow(technology);

        public long CostIfUnlockedNow(ITechnologyBlueprint technology) =>
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

            DebugAssert.Argument.Satisfies(() => CanUnlockTechnology(technology));

            queuedTechnologies.RemoveAt(0);
            unlockTechnology(technology);
        }

        private void unlockTechnology(ITechnologyBlueprint technology)
        {
            DebugAssert.Argument.Satisfies(() => !IsTechnologyQueued(technology));

            var cost = CostIfUnlockedNow(technology);
            TechPoints -= cost;
            techCostMultiplier *= TechCostMultiplicationFactor;
            unlockedTechnologies.Add(technology, cost);
            technology.Unlocks.ForEach(unlock => unlock.Apply(this));
            events.Send(new TechnologyUnlocked(technology));
        }

        public bool IsBuildingUnlocked(IBuildingBlueprint blueprint) => unlockedBuildings.Contains(blueprint);

        public void UnlockBuilding(IBuildingBlueprint blueprint)
        {
            if (unlockedBuildings.Add(blueprint))
            {
                events.Send(new BuildingTechnologyUnlocked(this, blueprint));
            }
        }

        public bool IsUpgradeUnlocked(IUpgradeBlueprint blueprint) => unlockedUpgrades.Contains(blueprint);

        public IEnumerable<IUpgradeBlueprint> GetApplicableUpgradesFor(Building building) =>
            unlockedUpgrades.Where(building.CanApplyUpgrade);

        public void UnlockUpgrade(IUpgradeBlueprint blueprint)
        {
            if (unlockedUpgrades.Add(blueprint))
            {
                events.Send(new UpgradeTechnologyUnlocked(this, blueprint));
            }
        }

        public void HandleEvent(EnemyKilled @event)
        {
            if (@event.KillingFaction?.Technology == this)
            {
                AddTechPoints(@event.Unit.Value);
            }
        }

        public void AddTechPoints(long number)
        {
            TechPoints += number;
            tryUnlockQueuedTechnologies();
        }
    }
}
