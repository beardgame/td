using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Resources;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class PlopBuilding
    {
        public static ICommand Command(GameInstance game, Faction faction, BuildingBlueprint blueprint, PositionedFootprint footprint)
            => new Implementation(game, faction, blueprint, footprint);

        private class Implementation : ICommand
        {
            private readonly GameInstance game;
            private readonly Faction faction;
            private readonly BuildingBlueprint blueprint;
            private readonly PositionedFootprint footprint;

            public Implementation(GameInstance game, Faction faction, BuildingBlueprint blueprint, PositionedFootprint footprint)
            {
                this.game = game;
                this.faction = faction;
                this.blueprint = blueprint;
                this.footprint = footprint;
            }

            public void Execute()
            {
                var building = new PlayerBuilding(blueprint, footprint, faction);
                game.State.Add(building);
                building.BuildManager.Progress(ResourceGrant.Infinite);
            }

            public ICommandSerializer Serializer => new Serializer(faction, blueprint, footprint);
        }

        private class Serializer : ICommandSerializer
        {
            private Id<Faction> faction;
            private Id<BuildingBlueprint> blueprint;
            private Id<Footprint> footprint;
            private int footprintX;
            private int footprintY;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(Faction faction, BuildingBlueprint blueprint, PositionedFootprint footprint)
            {
                this.faction = faction.Id;
                this.blueprint = blueprint.Id;
                this.footprint = footprint.Footprint.Id;
                footprintX = footprint.RootTile.X;
                footprintY = footprint.RootTile.Y;
            }

            public ICommand GetCommand(GameInstance game)
            {
                return new Implementation(
                    game,
                    game.State.FactionFor(faction),
                    game.Blueprints.Buildings[blueprint],
                    new PositionedFootprint(
                        game.State.Level,
                        game.Blueprints.Footprints[footprint],
                        new Tile<TileInfo>(game.State.Level.Tilemap, footprintX, footprintY)));
            }

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref faction);
                stream.Serialize(ref blueprint);
                stream.Serialize(ref footprint);
                stream.Serialize(ref footprintX);
                stream.Serialize(ref footprintY);
            }
        }
    }
}
