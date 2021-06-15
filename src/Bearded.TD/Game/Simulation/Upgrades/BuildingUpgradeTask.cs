using System;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Upgrades
{
    sealed class BuildingUpgradeTask : GameObject, IIdable<BuildingUpgradeTask>
    {
        private ResourceConsumer resourceConsumer = null!;

        public double ProgressPercentage => resourceConsumer.PercentageDone;

        public Id<BuildingUpgradeTask> Id { get; }
        public Building Building { get; }
        public IUpgradeBlueprint Upgrade { get; }

        public BuildingUpgradeTask(Id<BuildingUpgradeTask> id, Building building, IUpgradeBlueprint upgrade)
        {
            Id = id;
            Building = building;
            Upgrade = upgrade;
        }

        protected override void OnAdded()
        {
            Game.IdAs(this);
            if (!Building.Faction.TryGetBehaviorIncludingAncestors<FactionResources>(out var resources))
            {
                throw new NotSupportedException("Cannot build building without resources.");
            }
            resourceConsumer = new ResourceConsumer(
                Game,
                resources.ReserveResources(new FactionResources.ResourceRequest(Upgrade.Cost)),
                Constants.Game.Worker.UpgradeSpeed);
            Building.RegisterBuildingUpgradeTask(this);
            Game.Meta.Events.Send(new BuildingUpgradeQueued(Building, this));
        }

        protected override void OnDelete()
        {
            Building.UnregisterBuildingUpgradeTask(this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (!Building.IsBuildCompleted)
            {
                return;
            }

            if (resourceConsumer.IsDone)
            {
                return;
            }

            resourceConsumer.PrepareIfNeeded();
            resourceConsumer.Update();

            if (resourceConsumer.IsDone)
            {
                this.Sync(FinishBuildingUpgrade.Command, this);

                // Maybe don't Sync() this to consume right amount of resource on the client as well?
                // Test this well in that case though!
                Delete();
            }
        }

        public void Complete()
        {
            Building.ApplyUpgrade(Upgrade);
        }

        public override void Draw(CoreDrawers drawers) { }
    }
}
