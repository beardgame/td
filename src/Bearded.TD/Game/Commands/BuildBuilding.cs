using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Resources;
using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.Commands
{
    static class BuildBuilding
    {
        public static IRequest Request(GameState game, BuildingBlueprint blueprint, PositionedFootprint footprint)
            => new Implementation(game, blueprint, footprint);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly GameState game;
            private readonly BuildingBlueprint blueprint;
            private readonly PositionedFootprint footprint;

            public Implementation(GameState game, BuildingBlueprint blueprint, PositionedFootprint footprint)
            {
                this.game = game;
                this.blueprint = blueprint;
                this.footprint = footprint;
            }

            public override bool CheckPreconditions()
                => footprint.OccupiedTiles.All(tile => tile.IsValid && tile.Info.IsPassable);

            public override void Execute()
            {
                var building = new PlayerBuilding(blueprint, footprint);
                game.Add(building);
                game.Add(new DebugWorker(building.BuildManager));
            }

            protected override IUnifiedRequestCommandSerializer GetSerializer()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}