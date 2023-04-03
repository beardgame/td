using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.GameObjects;

static class SpawnGameObject
{
    public static ISerializableCommand<GameInstance> Command(
        GameState game,
        IGameObjectBlueprint blueprint,
        GameObject? owner,
        Position3 position,
        Direction2 direction) =>
        new Implementation(game, blueprint, owner, position, direction);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameState game;
        private readonly IGameObjectBlueprint blueprint;
        private readonly GameObject? owner;
        private readonly Position3 position;
        private readonly Direction2 direction;

        public Implementation(
            GameState game, IGameObjectBlueprint blueprint, GameObject? owner, Position3 position,
            Direction2 direction)
        {
            this.game = game;
            this.blueprint = blueprint;
            this.owner = owner;
            this.position = position;
            this.direction = direction;
        }

        public void Execute()
        {
            game.Add(
                GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(blueprint, owner, position, direction));
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer =>
            new Serializer(blueprint, owner, position, direction);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private ModAwareId blueprint;
        private Id<GameObject> owner;
        private Position3 position;
        private Direction2 direction;

        [UsedImplicitly] public Serializer() { }

        public Serializer(IGameObjectBlueprint blueprint, GameObject? owner, Position3 position, Direction2 direction)
        {
            this.blueprint = blueprint.Id;
            this.owner = owner?.FindId() ?? Id<GameObject>.Invalid;
            this.position = position;
            this.direction = direction;
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
        {
            return new Implementation(
                game.State,
                game.Blueprints.GameObjects[blueprint],
                owner.IsValid ? game.State.Find(owner) : null,
                position,
                direction
            );
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref blueprint);
            stream.Serialize(ref owner);
            stream.Serialize(ref position);
            stream.Serialize(ref direction);
        }
    }
}
