using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Technologies
{
    sealed class TechnologyManager : IListener<EnemyKilled>
    {
        private readonly GameEvents events;

        private readonly HashSet<ITechnologyBlueprint> unlockedTechnologies = new HashSet<ITechnologyBlueprint>();
        private readonly HashSet<IBuildingBlueprint> unlockedBuildings = new HashSet<IBuildingBlueprint>();
        private readonly HashSet<IUpgradeBlueprint> unlockedUpgrades = new HashSet<IUpgradeBlueprint>();
        private readonly List<ITechnologyBlueprint> queuedTechnologies = new List<ITechnologyBlueprint>();

        public long TechPoints { get; private set; }

        public IEnumerable<IBuildingBlueprint> UnlockedBuildings => unlockedBuildings.AsReadOnlyEnumerable();

        public TechnologyManager(GameEvents events)
        {
            this.events = events;

            events.Subscribe(this);
        }

        public bool IsTechnologyLocked(ITechnologyBlueprint technology) => !unlockedTechnologies.Contains(technology);

        public bool IsTechnologyQueued(ITechnologyBlueprint technology) => queuedTechnologies.Contains(technology);

        public int QueuePositionFor(ITechnologyBlueprint technologyBlueprint)
        {
            DebugAssert.Argument.Satisfies(() => IsTechnologyQueued(technologyBlueprint));
            return queuedTechnologies.FindIndex(t => t == technologyBlueprint) + 1;
        }

        public bool CanQueueTechnology(ITechnologyBlueprint technology) =>
            IsTechnologyLocked(technology)
            && !IsTechnologyQueued(technology)
            && (!canAfford(technology) || !hasAllRequiredTechs(technology));

        public void QueueTechnology(ITechnologyBlueprint technology)
        {
            DebugAssert.Argument.Satisfies(() => CanQueueTechnology(technology));
            queuedTechnologies.Add(technology);
            events.Send(new TechnologyQueued(technology));
        }

        public void DequeueTechnology(ITechnologyBlueprint technologyBlueprint)
        {
            DebugAssert.Argument.Satisfies(() => IsTechnologyQueued(technologyBlueprint));
            queuedTechnologies.Remove(technologyBlueprint);
        }

        public bool CanUnlockTechnology(ITechnologyBlueprint technology) =>
            IsTechnologyLocked(technology)
            && canAfford(technology)
            && hasAllRequiredTechs(technology);

        private bool canAfford(ITechnologyBlueprint technology) => TechPoints >= technology.Cost;

        private bool hasAllRequiredTechs(ITechnologyBlueprint technology) =>
            technology.RequiredTechs.All(unlockedTechnologies.Contains);

        public void UnlockTechnology(ITechnologyBlueprint technology)
        {
            DebugAssert.Argument.Satisfies(() => CanUnlockTechnology(technology));
            TechPoints -= technology.Cost;
            unlockedTechnologies.Add(technology);
            technology.Unlocks.ForEach(unlock => unlock.Apply(this));
            queuedTechnologies.RemoveAll(t => t == technology);
            events.Send(new TechnologyUnlocked(technology));
        }

        public void UnlockBuilding(IBuildingBlueprint blueprint)
        {
            if (unlockedBuildings.Add(blueprint))
            {
                events.Send(new BuildingTechnologyUnlocked(this, blueprint));
            }
        }

        public bool IsBuildingUnlocked(IBuildingBlueprint blueprint) => unlockedBuildings.Contains(blueprint);

        public void UnlockUpgrade(IUpgradeBlueprint blueprint)
        {
            if (unlockedUpgrades.Add(blueprint))
            {
                events.Send(new UpgradeTechnologyUnlocked(this, blueprint));
            }
        }

        public bool IsUpgradeUnlocked(IUpgradeBlueprint blueprint) => unlockedUpgrades.Contains(blueprint);

        public IEnumerable<IUpgradeBlueprint> GetApplicableUpgradesFor(Building building)
        {
            return unlockedUpgrades.Where(building.CanApplyUpgrade);
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
            while (queuedTechnologies.Count > 0 && queuedTechnologies[0].Cost <= TechPoints)
            {
                UnlockTechnology(queuedTechnologies[0]);
            }
        }
    }
}
