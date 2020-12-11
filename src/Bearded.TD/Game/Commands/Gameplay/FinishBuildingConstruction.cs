using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay
{
    static class FinishBuildingConstruction
    {
        public static ISerializableCommand<GameInstance> Command(Building building)
            => new Implementation(building);

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly Building building;

            public Implementation(Building building)
            {
                this.building = building;
            }

            public void Execute()
            {
                building.SetBuildCompleted();
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(building);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<Building> building;

            public Serializer(Building building)
            {
                this.building = building.Id;
            }

            [UsedImplicitly]
            public Serializer() { }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game.State.Find(building));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref building);
            }
        }
    }
}
