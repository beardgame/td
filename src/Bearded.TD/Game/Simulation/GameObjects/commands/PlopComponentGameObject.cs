using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.GameObjects;

static class PlopComponentGameObject
{
    public static ISerializableCommand<GameInstance> Command(GameInstance game, IComponentOwnerBlueprint blueprint, Position3 position, Direction2 direction)
        => new Implementation(game, blueprint, position, direction);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameInstance game;
        private readonly IComponentOwnerBlueprint blueprint;
        private readonly Position3 position;
        private readonly Direction2 direction;

        public Implementation(GameInstance game, IComponentOwnerBlueprint blueprint, Position3 position, Direction2 direction)
        {
            this.game = game;
            this.blueprint = blueprint;
            this.position = position;
            this.direction = direction;
        }

        public void Execute()
        {
            ComponentGameObjectFactory.CreateFromBlueprintWithDefaultRenderer(game.State, blueprint, null, position, direction);
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(blueprint, position, direction);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private ModAwareId blueprint;
        private Position3 position;
        private Direction2 direction;

        [UsedImplicitly]
        public Serializer()
        {
        }

        public Serializer(IComponentOwnerBlueprint blueprint, Position3 position, Direction2 direction)
        {
            this.blueprint = blueprint.Id;
            this.position = position;
            this.direction = direction;
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
        {
            return new Implementation(
                game,
                game.Blueprints.ComponentOwners[blueprint],
                position,
                direction
            );
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref blueprint);
            stream.Serialize(ref position);
            stream.Serialize(ref direction);
        }
    }
}
