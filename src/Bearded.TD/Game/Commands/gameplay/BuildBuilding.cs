using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.World;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class BuildBuilding
    {
        public static IRequest<GameInstance> Request(GameInstance game, Faction faction, IBuildingBlueprint blueprint, PositionedFootprint footprint)
            => new Implementation(game, faction, Id<BuildingPlaceholder>.Invalid, blueprint, footprint);
        
        private class Implementation : UnifiedRequestCommand
        {
            private readonly GameInstance game;
            private readonly Faction faction;
            private readonly Id<BuildingPlaceholder> id;
            private readonly IBuildingBlueprint blueprint;
            private readonly PositionedFootprint footprint;

            public Implementation(GameInstance game, Faction faction, Id<BuildingPlaceholder> id, IBuildingBlueprint blueprint, PositionedFootprint footprint)
            {
                this.game = game;
                this.faction = faction;
                this.id = id;
                this.blueprint = blueprint;
                this.footprint = footprint;
            }

            public override bool CheckPreconditions()
                => game.State.BuildingPlacementLayer.AreTilesValidForBuilding(footprint.OccupiedTiles)
                        && blueprint.FootprintGroup == footprint.Footprint;

            public override ISerializableCommand<GameInstance> ToCommand() => new Implementation(game, faction, game.Meta.Ids.GetNext<BuildingPlaceholder>(), blueprint, footprint);

            public override void Execute()
            {
                var placeholder = new BuildingPlaceholder(id, blueprint, faction, footprint);
                game.State.Add(placeholder);
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(faction, id, blueprint, footprint);
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<Faction> faction;
            private string blueprint;
            private string footprint;
            private int footprintIndex;
            private Id<BuildingPlaceholder> id;
            private int footprintX;
            private int footprintY;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(Faction faction, Id<BuildingPlaceholder> id, IBlueprint blueprint, PositionedFootprint footprint)
            {
                this.id = id;
                this.faction = faction.Id;
                this.blueprint = blueprint.Id;
                this.footprint = footprint.Footprint.Id;
                footprintIndex = footprint.FootprintIndex;
                footprintX = footprint.RootTile.X;
                footprintY = footprint.RootTile.Y;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
            {
                return new Implementation(
                    game,
                    game.State.FactionFor(faction),
                    id,
                    game.Blueprints.Buildings[blueprint],
                    new PositionedFootprint(
                        game.State.Level,
                        game.Blueprints.Footprints[footprint], footprintIndex,
                        new Tile(footprintX, footprintY)));
            }

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref faction);
                stream.Serialize(ref id);
                stream.Serialize(ref blueprint);
                stream.Serialize(ref footprint);
                stream.Serialize(ref footprintIndex);
                stream.Serialize(ref footprintX);
                stream.Serialize(ref footprintY);
            }
        }
    }
}
