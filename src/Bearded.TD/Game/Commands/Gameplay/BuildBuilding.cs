using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Resources;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class BuildBuilding
    {
        public static IRequest Request(GameInstance game, BuildingBlueprint blueprint, PositionedFootprint footprint)
            => new Implementation(game, blueprint, footprint);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly GameInstance game;
            private readonly BuildingBlueprint blueprint;
            private readonly PositionedFootprint footprint;

            public Implementation(GameInstance game, BuildingBlueprint blueprint, PositionedFootprint footprint)
            {
                this.game = game;
                this.blueprint = blueprint;
                this.footprint = footprint;
            }

            public override bool CheckPreconditions()
                // TODO: check if the positionedfootprint matches the building blueprint conditions
                => footprint.OccupiedTiles.All(tile => tile.IsValid && tile.Info.IsPassable);

            public override void Execute()
            {
                var building = new PlayerBuilding(blueprint, footprint);
                game.State.Add(building);
                game.State.Add(new DebugWorker(building.BuildManager));
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(blueprint, footprint);
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<BuildingBlueprint> blueprint;
            private PositionedFootprint footprint;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(BuildingBlueprint blueprint, PositionedFootprint footprint)
            {
                this.blueprint = blueprint.Id;
                this.footprint = footprint;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
            {
                return new Implementation(game, game.Blueprints.Buildings[blueprint], footprint);
            }

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref blueprint);
                // TODO: serialize footprint
            }
        }
    }
}
