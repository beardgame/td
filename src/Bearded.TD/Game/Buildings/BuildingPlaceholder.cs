using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
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
        public override int Health => 0;

        public BuildingPlaceholder(Id<BuildingPlaceholder> id, BuildingBlueprint blueprint, Faction faction, PositionedFootprint footprint)
            : base(blueprint, faction, footprint)
        {
            Id = id;
            workerTask = new BuildingWorkerTask(this);
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
            Game.Meta.Events.StartBuildingConstruction(this, building);
        }

        public override void Draw(GeometryManager geometries)
        {
            DrawTiles(geometries, Color.Cyan * 0.25f);
            base.Draw(geometries);
        }
    }
}
