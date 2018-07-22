using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.World;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class PlopBuilding
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, Faction faction, Id<Building> id, IBuildingBlueprint blueprint, PositionedFootprint footprint)
            => new Implementation(game, faction, id, blueprint, footprint);

        private class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;
            private readonly Faction faction;
            private readonly Id<Building> id;
            private readonly IBuildingBlueprint blueprint;
            private readonly PositionedFootprint footprint;

            public Implementation(GameInstance game, Faction faction, Id<Building> id, IBuildingBlueprint blueprint, PositionedFootprint footprint)
            {
                this.game = game;
                this.faction = faction;
                this.id = id;
                this.blueprint = blueprint;
                this.footprint = footprint;
            }

            public void Execute()
            {
                var building = new Building(id, blueprint, faction, footprint);
                game.State.Add(building);
                building.SetBuildProgress(1, building.Blueprint.MaxHealth - 1);
                building.SetBuildCompleted();
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(faction, id, blueprint, footprint);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<Building> id;
            private Id<Faction> faction;
            private string blueprint;
            private string footprint;
            private int footprintIndex;
            private int footprintX;
            private int footprintY;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(Faction faction, Id<Building> id, IBlueprint blueprint, PositionedFootprint footprint)
            {
                this.id = id;
                this.faction = faction.Id;
                this.blueprint = blueprint.Id;
                this.footprint = footprint.Footprint.Id;
                footprintX = footprint.RootTile.X;
                footprintY = footprint.RootTile.Y;
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            {
                return new Implementation(
                    game,
                    game.State.FactionFor(faction),
                    id,
                    game.Blueprints.Buildings[blueprint],
                    new PositionedFootprint(
                        game.State.Level,
                        game.Blueprints.Footprints[footprint], footprintIndex,
                        new Tile<TileInfo>(game.State.Level.Tilemap, footprintX, footprintY)));
            }

            public void Serialize(INetBufferStream stream)
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
