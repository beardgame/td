using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Events;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using static Bearded.TD.Constants.Game.Technology;

namespace Bearded.TD.Game.Technologies
{
    sealed class TechnologyManager : IListener<EnemyKilled>
    {
        private readonly GlobalGameEvents events;

        private readonly HashSet<ITechnologyBlueprint> unlockedTechnologies = new HashSet<ITechnologyBlueprint>();
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

        public bool IsTechnologyLocked(ITechnologyBlueprint technology) => !unlockedTechnologies.Contains(technology);

        public bool CanUnlockTechnology(ITechnologyBlueprint technology) =>
            IsTechnologyLocked(technology)
            && canAffordNow(technology)
            && HasAllRequiredTechs(technology);

        private bool canAffordNow(ITechnologyBlueprint technology) => TechPoints >= CostIfUnlockedNow(technology);

        public long CostIfUnlockedNow(ITechnologyBlueprint technology)
        {
            return (long) (technology.Cost * techCostMultiplier);
        }

        public long CostAtEndOfQueue(ITechnologyBlueprint technology)
        {
            var multiplier = techCostMultiplier * Math.Pow(TechCostMultiplicationFactor, queuedTechnologies.Count);
            return (long) (technology.Cost * multiplier);
        }

        public bool HasAllRequiredTechs(ITechnologyBlueprint technology) =>
            technology.RequiredTechs.All(unlockedTechnologies.Contains);

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

            TechPoints -= CostIfUnlockedNow(technology);
            techCostMultiplier *= TechCostMultiplicationFactor;
            unlockedTechnologies.Add(technology);
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
            if (@event.KillingFaction.Technology == this)
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
