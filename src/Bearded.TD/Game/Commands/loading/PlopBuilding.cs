using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class PlopBuilding
    {
        public static ICommand Command(GameInstance game, Faction faction, Id<Building> id, BuildingBlueprint blueprint, PositionedFootprint footprint)
            => new Implementation(game, faction, id, blueprint, footprint);

        private class Implementation : ICommand
        {
            private readonly GameInstance game;
            private readonly Faction faction;
            private readonly Id<Building> id;
            private readonly BuildingBlueprint blueprint;
            private readonly PositionedFootprint footprint;

            public Implementation(GameInstance game, Faction faction, Id<Building> id, BuildingBlueprint blueprint, PositionedFootprint footprint)
            {
                this.game = game;
                this.faction = faction;
                this.id = id;
                this.blueprint = blueprint;
                this.footprint = footprint;
            }

            public void Execute()
            {
                var building = new PlayerBuilding(id, blueprint, footprint, faction);
                game.State.Add(building);
                building.ResetToComplete();
            }

            public ICommandSerializer Serializer => new Serializer(faction, id, blueprint, footprint);
        }

        private class Serializer : ICommandSerializer
        {
            private Id<Building> id;
            private Id<Faction> faction;
            private string blueprint;
            private string footprint;
            private int footprintX;
            private int footprintY;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(Faction faction, Id<Building> id, BuildingBlueprint blueprint, PositionedFootprint footprint)
            {
                this.id = id;
                this.faction = faction.Id;
                this.blueprint = blueprint.Name;
                this.footprint = footprint.Footprint.Name;
                footprintX = footprint.RootTile.X;
                footprintY = footprint.RootTile.Y;
            }

            public ICommand GetCommand(GameInstance game)
            {
                return new Implementation(
                    game,
                    game.State.FactionFor(faction),
                    id,
                    game.Blueprints.Buildings[blueprint],
                    new PositionedFootprint(
                        game.State.Level,
                        game.Blueprints.Footprints[footprint],
                        new Tile<TileInfo>(game.State.Level.Tilemap, footprintX, footprintY)));
            }

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref faction);
                stream.Serialize(ref id);
                stream.Serialize(ref blueprint);
                stream.Serialize(ref footprint);
                stream.Serialize(ref footprintX);
                stream.Serialize(ref footprintY);
            }
        }
    }
}
