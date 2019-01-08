using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Resources;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Upgrades
{
    class BuildingUpgradeTask : GameObject, IResourceConsumer
    {
        private readonly Building building;
        private readonly UpgradeBlueprint upgrade;

        private double maximumResources => upgrade.Cost;
        
        private double progress;
        private bool completed;

        public Building Building => building;
        public double ProgressPercentage => progress / maximumResources;
        public UpgradeBlueprint Upgrade => upgrade;

        public BuildingUpgradeTask(Building building, UpgradeBlueprint upgrade)
        {
            this.building = building;
            this.upgrade = upgrade;
        }

        protected override void OnAdded()
        {
            building.RegisterBuildingUpgradeTask(this);
        }

        protected override void OnDelete()
        {
            building.UnregisterBuildingUpgradeTask(this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (!building.IsCompleted)
                return;
            
            if (completed)
                return;
            
            building.Faction.Resources.RegisterConsumer(this, 10, maximumResources - progress);
        }

        public override void Draw(GeometryManager geometries)
        {
        }

        public void ConsumeResources(ResourceGrant grant)
        {
            progress += grant.Amount;
            completed = grant.ReachedCapacity;

            if (completed)
            {
                building.Sync(() => FinishBuildingUpgrade.Command(building, upgrade));
                
                // Maybe don't Sync() this to consume right amount of resource on the client as well?
                // Test this well in that case though!
                Delete();
            }
        }
    }
}
