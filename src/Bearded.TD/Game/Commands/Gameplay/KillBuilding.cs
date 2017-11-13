using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class KillBuilding
    {
        public static ICommand<GameInstance> Command(Building building) => new Implementation(building);

        private class Implementation : ICommand<GameInstance>
        {
            private readonly Building building;

            public Implementation(Building building)
            {
                this.building = building;
            }

            public void Execute() => building.Delete();
            public ICommandSerializer<GameInstance> Serializer => new Serializer(building);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<Building> building;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(Building building)
            {
                this.building = building.Id;
            }

            public ICommand<GameInstance> GetCommand(GameInstance game)
            {
                return new Implementation(game.State.Find(building));
            }

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref building);
            }
        }
    }
}
