using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Loading
{
    static class PlopBuilding
    {
        public static ISerializableCommand<GameInstance> Command(
                GameState gameState,
                Faction faction,
                Id<Building> id,
                IComponentOwnerBlueprint blueprint,
                PositionedFootprint footprint)
            => new Implementation(gameState, faction, id, blueprint, footprint);

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameState gameState;
            private readonly Faction faction;
            private readonly Id<Building> id;
            private readonly IComponentOwnerBlueprint blueprint;
            private readonly PositionedFootprint footprint;

            public Implementation(GameState gameState, Faction faction, Id<Building> id, IComponentOwnerBlueprint blueprint, PositionedFootprint footprint)
            {
                this.gameState = gameState;
                this.faction = faction;
                this.id = id;
                this.blueprint = blueprint;
                this.footprint = footprint;
            }

            public void Execute()
            {
                var building = new BuildingFactory(gameState).Create(id, blueprint, faction, footprint);
                var constructionSyncer = building.GetComponents<IBuildingConstructionSyncer>().Single();
                constructionSyncer.SyncStartBuild();
                constructionSyncer.SyncCompleteBuild();
            }

            ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(faction, id, blueprint, footprint);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<Building> id;
            private Id<Faction> faction;
            private ModAwareId blueprint;
            private ModAwareId footprint;
            private int footprintIndex;
            private int footprintX;
            private int footprintY;

            [UsedImplicitly]
            public Serializer() {}

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
                    game.State.Factions.Resolve(faction),
                    id,
                    game.Blueprints.ComponentOwners[blueprint],
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
