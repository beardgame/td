using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Upgrades
{
    sealed class BuildingUpgradeTask : GameObject
    {
        private readonly Building building;
        private readonly IUpgradeBlueprint upgrade;
        private ResourceConsumer resourceConsumer = null!;

        public Building Building => building;
        public double ProgressPercentage => resourceConsumer.PercentageDone;
        public IUpgradeBlueprint Upgrade => upgrade;

        public BuildingUpgradeTask(Building building, IUpgradeBlueprint upgrade)
        {
            this.building = building;
            this.upgrade = upgrade;
        }

        protected override void OnAdded()
        {
            resourceConsumer = new ResourceConsumer(
                Game,
                building.Faction.Resources.ReserveResources(new ResourceManager.ResourceRequest(upgrade.Cost)),
                Constants.Game.Worker.UpgradeSpeed);
            building.RegisterBuildingUpgradeTask(this);
        }

        protected override void OnDelete()
        {
            building.UnregisterBuildingUpgradeTask(this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (!building.IsCompleted)
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
                building.Sync(FinishBuildingUpgrade.Command, building, upgrade);

                // Maybe don't Sync() this to consume right amount of resource on the client as well?
                // Test this well in that case though!
                Delete();
            }
        }

        public override void Draw(GeometryManager geometries) { }
    }
}
