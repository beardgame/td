using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Commands
{
    static class CreateGameState
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, int radius)
            => new Implementation(game, radius);

        private class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;
            private readonly int radius;

            public Implementation(GameInstance game, int radius)
            {
                this.game = game;
                this.radius = radius;
            }

            public void Execute()
            {
                var state = new GameState(game.Meta, radius);

                foreach (var blueprint in game.Blueprints.Buildings.All)
                {
                    state.Technology.UnlockBlueprint(blueprint);
                }
                state.Technology.UnlockUpgrade(new UpgradeBlueprint("+25% damage",
                    new[] {new ParameterModifiable(AttributeType.Damage, Modification.AddFractionOfBase(.25))}));

                game.InitialiseState(state);
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(radius);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private int radius;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(int radius)
            {
                this.radius = radius;
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game, radius);

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref radius);
            }
        }
    }
}
