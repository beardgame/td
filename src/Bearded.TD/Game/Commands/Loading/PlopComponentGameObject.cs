using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Commands.Loading
{
    static class PlopComponentGameObject
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, IComponentOwnerBlueprint blueprint, Position3 position, Direction2 direction)
            => new Implementation(game, blueprint, position, direction);

        private class Implementation : ISerializableCommand<GameInstance>
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
                var obj = new ComponentGameObject(blueprint, null, position, direction);
                game.State.Add(obj);
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(blueprint, position, direction);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private ModAwareId blueprint;
            private Position3 position;
            private Direction2 direction;

            // ReSharper disable once UnusedMember.Local
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
}
