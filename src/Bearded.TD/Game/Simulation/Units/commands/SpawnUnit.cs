using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay;

static class SpawnUnit
{
    public static ISerializableCommand<GameInstance> Command(
        GameState game, Tile tile, IComponentOwnerBlueprint blueprint, Id<ComponentGameObject> unitId)
        => new Implementation(game, tile, blueprint, unitId);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameState game;
        private readonly Tile tile;
        private readonly IComponentOwnerBlueprint blueprint;
        private readonly Id<ComponentGameObject> unitId;

        public Implementation(
            GameState game, Tile tile, IComponentOwnerBlueprint blueprint, Id<ComponentGameObject> unitId)
        {
            this.game = game;
            this.tile = tile;
            this.blueprint = blueprint;
            this.unitId = unitId;
        }

        public void Execute() => EnemyUnitFactory.Create(game, unitId, blueprint, tile);

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer =>
            new Serializer(blueprint, tile, unitId);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private ModAwareId blueprint;
        private int x;
        private int y;
        private Id<ComponentGameObject> unitId;

        [UsedImplicitly]
        public Serializer()
        {
        }

        public Serializer(IBlueprint blueprint, Tile tile, Id<ComponentGameObject> unitId)
        {
            this.blueprint = blueprint.Id;
            x = tile.X;
            y = tile.Y;
            this.unitId = unitId;
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game) => new Implementation(
            game.State,
            new Tile(x, y),
            game.Blueprints.ComponentOwners[blueprint],
            unitId);

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref blueprint);
            stream.Serialize(ref x);
            stream.Serialize(ref y);
            stream.Serialize(ref unitId);
        }
    }
}
