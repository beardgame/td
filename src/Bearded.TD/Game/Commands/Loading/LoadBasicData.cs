using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Buildings.Components;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    static class LoadBasicData
    {
        public static ICommand Command(GameInstance game)
            => new Implementation(game);

        private class Implementation : ICommand
        {
            private readonly GameInstance game;

            public Implementation(GameInstance game)
            {
                this.game = game;
            }

            public void Execute()
            {
                // Footprints
                var footprints = game.Blueprints.Footprints;
                footprints.RegisterBlueprint(Footprint.Single);
                footprints.RegisterBlueprint(Footprint.TriangleUp);
                footprints.RegisterBlueprint(Footprint.TriangleDown);
                footprints.RegisterBlueprint(Footprint.CircleSeven);

                // Components
                var components = game.Blueprints.Components;
                components.RegisterBlueprint(new ComponentFactory(new Utilities.Id<ComponentFactory>(1), "turret", () => new Turret()));
            }

            public ICommandSerializer Serializer => new Serializer();
        }

        private class Serializer : ICommandSerializer
        {
            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public ICommand GetCommand(GameInstance game)
                => new Implementation(game);

            public void Serialize(INetBufferStream stream)
            {
            }
        }
    }
}
