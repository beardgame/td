using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.UI
{
    class BuildingInteractionHandler : InteractionHandler
    {
        private readonly Faction faction;
        private readonly BuildingBlueprint blueprint;
        protected override TileSelection TileSelection { get; }
        private BuildingGhost ghost;

        public BuildingInteractionHandler(GameInstance game, Faction faction, BuildingBlueprint blueprint) : base(game)
        {
            this.faction = faction;
            this.blueprint = blueprint;
            TileSelection = TileSelection.FromFootprints(blueprint.Footprints);
        }

        protected override void OnStart(ICursorHandler cursor)
        {
            ghost = new BuildingGhost(blueprint);
            Game.State.Add(ghost);
            ghost.SetFootprint(cursor.CurrentFootprint);
        }

        public override void Update(UpdateEventArgs args, ICursorHandler cursor)
        {
            var footprint = cursor.CurrentFootprint;
            ghost.SetFootprint(footprint);
            if (cursor.ClickAction.Hit)
                Game.Request(BuildBuilding.Request, faction, blueprint, footprint);
        }

        protected override void OnEnd(ICursorHandler cursor)
        {
            ghost.Delete();
        }
    }
}
