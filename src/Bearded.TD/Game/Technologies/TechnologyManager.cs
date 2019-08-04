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

        public long TechPoints { get; private set; }

        public TechnologyManager(GameEvents events)
        {
            this.events = events;

            events.Subscribe(this);
        }

        public bool IsTechnologyLocked(ITechnologyBlueprint technology) => !unlockedTechnologies.Contains(technology);

        public void UnlockTechnology(ITechnologyBlueprint technology)
        {
            DebugAssert.Argument.Satisfies(() => IsTechnologyLocked(technology));
            DebugAssert.Argument.Satisfies(() => TechPoints > technology.Cost);
            TechPoints -= technology.Cost;
            unlockedTechnologies.Add(technology);
            technology.Effects.ForEach(effect => effect.Unlock(this));
            events.Send(new TechnologyUnlocked(technology));
        }

        public void UnlockBuilding(IBuildingBlueprint blueprint)
        {
            unlockedBuildings.Add(blueprint);
            events.Send(new BuildingTechnologyUnlocked(blueprint));
        }

        public void UnlockUpgrade(IUpgradeBlueprint blueprint)
        {
            unlockedUpgrades.Add(blueprint);
            events.Send(new UpgradeTechnologyUnlocked(blueprint));
        }

        public IEnumerable<IUpgradeBlueprint> GetApplicableUpgradesFor(Building building)
        {
            return unlockedUpgrades.Where(building.CanApplyUpgrade);
        }

        public void HandleEvent(EnemyKilled @event)
        {
            if (@event.KillingFaction.Technology == this)
            {
                TechPoints += @event.Unit.Value;
            }
        }

        public void AddTechPoints(long number)
        {
            TechPoints += number;
        }
    }
}
