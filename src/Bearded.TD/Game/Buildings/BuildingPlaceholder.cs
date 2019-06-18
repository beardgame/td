using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Events;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Workers;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Buildings
{
    [ComponentOwner]
    class BuildingPlaceholder : PlacedBuildingBase<BuildingPlaceholder>, IIdable<BuildingPlaceholder>
    {
        public Id<BuildingPlaceholder> Id { get; }
        private readonly BuildingWorkerTask workerTask;

        public BuildingPlaceholder(
            Id<BuildingPlaceholder> id,
            IBuildingBlueprint blueprint,
            Faction faction,
            PositionedFootprint footprint,
            Id<IWorkerTask> taskId) : base(blueprint, faction, footprint)
        {
            Id = id;
            workerTask = new BuildingWorkerTask(taskId, this);
        }

        protected override IEnumerable<IComponent<BuildingPlaceholder>> InitialiseComponents()
            => Blueprint.GetComponentsForPlaceholder();

        protected override void OnAdded()
        {
            base.OnAdded();

            Game.IdAs(this);
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

        public override void Draw(GeometryManager geometries)
        {
            DrawTiles(geometries, Color.Cyan * 0.25f);
            base.Draw(geometries);
        }
    }
}
