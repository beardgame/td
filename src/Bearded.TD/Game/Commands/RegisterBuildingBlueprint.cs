using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Networking.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Bearded.TD.Game.Commands
{
    static class RegisterBuildingBlueprint
    {
        public static ICommand Command(GameInstance game, string name, BuildingBlueprint blueprint)
            => new Implementation(game, name, blueprint);

        private class Implementation : ICommand
        {
            private readonly GameInstance game;
            private readonly string name;
            private readonly BuildingBlueprint blueprint;

            public Implementation(GameInstance game, string name, BuildingBlueprint blueprint)
            {
                this.game = game;
                this.blueprint = blueprint;
            }

            public void Execute()
            {
                game.Blueprints.Buildings.RegisterBlueprint(name, blueprint);
            }

            public ICommandSerializer Serializer => new Serializer(name, blueprint);
        }

        private class Serializer : ICommandSerializer
        {
            private readonly string name;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(string name, BuildingBlueprint blueprint)
            {
                this.name = name;
            }

            public ICommand GetCommand(GameInstance game)
                => new Implementation(game);

            public void Serialize(INetBufferStream stream)
            {
            }
        }
    }
}