using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Game.Input
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
            TileSelection = TileSelection.FromFootprints(blueprint.FootprintGroup);
        }

        protected override void OnStart(ICursorHandler cursor)
        {
            ghost = new BuildingGhost(blueprint, faction, cursor.CurrentFootprint);
            Game.State.Add(ghost);
        }

        public override void Update(UpdateEventArgs args, ICursorHandler cursor)
        {
            var footprint = cursor.CurrentFootprint;
            ghost.SetFootprint(footprint);
            if (cursor.Click.Hit)
                Game.Request(BuildBuilding.Request, faction, blueprint, footprint);
        }

        protected override void OnEnd(ICursorHandler cursor)
        {
            ghost.Delete();
        }
    }
}
