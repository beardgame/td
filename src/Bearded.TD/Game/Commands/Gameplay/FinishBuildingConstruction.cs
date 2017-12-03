using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class FinishBuildingConstruction
    {
        public static ICommand Command(Building building)
            => new Implementation(building);

        private class Implementation : ICommand
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

            public ICommandSerializer Serializer => new Serializer(building);
        }

        private class Serializer : ICommandSerializer
        {
            private Id<Building> building;

            public Serializer(Building building)
            {
                this.building = building.Id;
            }

            // ReSharper disable once UnusedMember.Local
            public Serializer() { }

            public ICommand GetCommand(GameInstance game)
                => new Implementation(game.State.Find(building));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref building);
            }
        }
    }
}
