using System.Collections.Generic;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Buildings
{
    [ComponentOwner]
    sealed class BuildingPlaceholder : PlacedBuildingBase<BuildingPlaceholder>, IIdable<BuildingPlaceholder>
    {
        public Id<BuildingPlaceholder> Id { get; }
        private readonly Id<IWorkerTask> taskId;
        private BuildingWorkerTask workerTask = null!;

        public BuildingPlaceholder(
            Id<BuildingPlaceholder> id,
            IBuildingBlueprint blueprint,
            Faction faction,
            PositionedFootprint footprint,
            Id<IWorkerTask> taskId) : base(blueprint, faction, footprint)
        {
            Id = id;
            this.taskId = taskId;
        }

        protected override IEnumerable<IComponent<BuildingPlaceholder>> InitializeComponents()
            => Blueprint.GetComponentsForPlaceholder();

        protected override void OnAdded()
        {
            base.OnAdded();

            Game.IdAs(this);

            workerTask = new BuildingWorkerTask(
                taskId,
                this,
                Faction.Resources.ReserveResources(new ResourceManager.ResourceRequest(Blueprint.ResourceCost)));
            Faction.Workers.RegisterTask(workerTask);
        }

        public void StartBuild(Id<Building> buildingId)
        {
            Delete();
            var building = new Building(buildingId, Blueprint, Faction, Footprint);
            Game.Add(building);
            workerTask.SetBuilding(building);
            Game.Meta.Events.Send(new BuildingConstructionStarted(this, building));
        }

        public IRequest<Player, GameInstance> CancelRequest()
        {
            return AbortTask.Request(Faction, workerTask);
        }

        public override void Draw(CoreDrawers geometries)
        {
            //DrawTiles(geometries, Color.Cyan * 0.25f);
            base.Draw(geometries);
        }
    }
}
