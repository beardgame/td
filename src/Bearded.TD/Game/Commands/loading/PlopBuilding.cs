using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.GameState;
using Bearded.TD.Game.GameState.Buildings;
using Bearded.TD.Game.GameState.Components.Damage;
using Bearded.TD.Game.GameState.Factions;
using Bearded.TD.Game.GameState.World;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class PlopBuilding
    {
        public static ISerializableCommand<GameInstance> Command(
                GameState.GameState gameState,
                Faction faction,
                Id<Building> id,
                IBuildingBlueprint blueprint,
                PositionedFootprint footprint)
            => new Implementation(gameState, faction, id, blueprint, footprint);

        private class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameState.GameState gameState;
            private readonly Faction faction;
            private readonly Id<Building> id;
            private readonly IBuildingBlueprint blueprint;
            private readonly PositionedFootprint footprint;

            public Implementation(GameState.GameState gameState, Faction faction, Id<Building> id, IBuildingBlueprint blueprint, PositionedFootprint footprint)
            {
                this.gameState = gameState;
                this.faction = faction;
                this.id = id;
                this.blueprint = blueprint;
                this.footprint = footprint;
            }

            public void Execute()
            {
                var building = new Building(id, blueprint, faction, footprint);
                gameState.Add(building);
                building.GetComponents<Health<Building>>()
                    .MaybeSingle()
                    .Match(health => building.SetBuildProgress(1, health.MaxHealth - 1));
                building.SetBuildCompleted();
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(faction, id, blueprint, footprint);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<Building> id;
            private Id<Faction> faction;
            private ModAwareId blueprint;
            private ModAwareId footprint;
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
                    game.State,
                    game.State.FactionFor(faction),
                    id,
                    game.Blueprints.Buildings[blueprint],
                    new PositionedFootprint(
                        game.Blueprints.Footprints[footprint], footprintIndex,
                        new Tile(footprintX, footprintY)));
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
