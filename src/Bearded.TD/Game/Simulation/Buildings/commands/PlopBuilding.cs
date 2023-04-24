using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Buildings;

static class PlopBuilding
{
    public static ISerializableCommand<GameInstance> Command(
        GameState gameState,
        Faction faction,
        Id<GameObject> id,
        IGameObjectBlueprint blueprint,
        PositionedFootprint footprint)
        => new Implementation(gameState, faction, id, blueprint, footprint);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameState gameState;
        private readonly Faction faction;
        private readonly Id<GameObject> id;
        private readonly IGameObjectBlueprint blueprint;
        private readonly PositionedFootprint footprint;

        public Implementation(
            GameState gameState,
            Faction faction,
            Id<GameObject> id,
            IGameObjectBlueprint blueprint,
            PositionedFootprint footprint)
        {
            this.gameState = gameState;
            this.faction = faction;
            this.id = id;
            this.blueprint = blueprint;
            this.footprint = footprint;
        }

        public void Execute()
        {
            var building = BuildingFactory.Create(id, blueprint, faction, footprint);
            gameState.Add(building);
            var constructionSyncer = building.GetComponents<IBuildingConstructionSyncer>().Single();
            constructionSyncer.SyncStartBuild();
            constructionSyncer.SyncCompleteBuild();
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer =>
            new Serializer(faction, id, blueprint, footprint);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<GameObject> id;
        private Id<Faction> faction;
        private ModAwareId blueprint;
        private ModAwareId footprint;
        private int footprintX;
        private int footprintY;

        [UsedImplicitly]
        public Serializer() {}

        public Serializer(Faction faction, Id<GameObject> id, IBlueprint blueprint, PositionedFootprint footprint)
        {
            this.id = id;
            this.faction = faction.Id;
            this.blueprint = blueprint.Id;
            this.footprint = footprint.Footprint?.Id ?? ModAwareId.Invalid;
            footprintX = footprint.RootTile.X;
            footprintY = footprint.RootTile.Y;
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
        {
            return new Implementation(
                game.State,
                game.State.Factions.Resolve(faction),
                id,
                game.Blueprints.GameObjects[blueprint],
                new PositionedFootprint(
                    footprint.IsValid ? game.Blueprints.Footprints[footprint] : PositionedFootprint.Invalid,
                    new Tile(footprintX, footprintY)));
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
