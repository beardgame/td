using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Footprints;

namespace Bearded.TD.Game.Input
{
    sealed class BuildingInteractionHandler : InteractionHandler
    {
        private readonly Faction faction;
        private readonly IBuildingBlueprint blueprint;
        protected override TileSelection TileSelection { get; }
        private BuildingGhost? ghost;
        private MovableTileOccupation<BuildingGhost>? ghostTileOccupation;

        public BuildingInteractionHandler(GameInstance game, Faction faction, IBuildingBlueprint blueprint) : base(game)
        {
            this.faction = faction;
            this.blueprint = blueprint;
            TileSelection = TileSelection.FromFootprints(blueprint.GetFootprintGroup());
        }

        protected override void OnStart(ICursorHandler cursor)
        {
            ghost = new BuildingGhost(blueprint, faction);
            Game.State.Add(ghost);
            ghostTileOccupation = new MovableTileOccupation<BuildingGhost>();
            ghost.AddComponent(ghostTileOccupation);
        }

        public override void Update(ICursorHandler cursor)
        {
            var footprint = cursor.CurrentFootprint;
            ghostTileOccupation?.SetFootprint(footprint);
            if (cursor.Click.Hit)
            {
                Game.Request(BuildBuilding.Request, faction, blueprint, footprint);
            }
            else if (cursor.Cancel.Hit)
            {
                Game.PlayerInput.ResetInteractionHandler();
            }
        }

        protected override void OnEnd(ICursorHandler cursor)
        {
            ghost?.Delete();
            ghost = null;
            ghostTileOccupation = null;
        }
    }
}
