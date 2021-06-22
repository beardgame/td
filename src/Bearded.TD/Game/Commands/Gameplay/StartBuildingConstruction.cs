using System;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay
{
    static class StartBuildingConstruction
    {
        public static ISerializableCommand<GameInstance> Command(Building building)
            => new CommandImplementation(building);

        private sealed class CommandImplementation : ISerializableCommand<GameInstance>
        {
            private readonly Building building;

            public CommandImplementation(Building building)
            {
                this.building = building;
            }

            public void Execute()
            {
                building.Materialize();
                (building.GetComponents<BuildingConstructionWork>().FirstOrDefault() ??
                    throw new NotSupportedException()).StartWork();
            }

            public ICommandSerializer<GameInstance> Serializer => new CommandSerializer(building);
        }

        private sealed class CommandSerializer : ICommandSerializer<GameInstance>
        {
            private Id<Building> buildingId;

            public CommandSerializer(Building building)
            {
                buildingId = building.Id;
            }

            [UsedImplicitly]
            public CommandSerializer() { }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new CommandImplementation(game.State.Find(buildingId));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref buildingId);
            }
        }
    }
}
