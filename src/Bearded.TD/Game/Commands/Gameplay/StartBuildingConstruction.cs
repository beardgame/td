using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class StartBuildingConstruction
    {
        public static ICommand Command(BuildingPlaceholder placeholder)
            => new CommandImplementation(
                placeholder.Game.Meta.Ids.GetNext<Building>(), placeholder);

        private class CommandImplementation : ICommand
        {
            private readonly Id<Building> buildingId;
            private readonly BuildingPlaceholder placeholder;

            public CommandImplementation(
                Id<Building> buildingId,
                BuildingPlaceholder placeholder)
            {
                this.buildingId = buildingId;
                this.placeholder = placeholder;
            }

            public void Execute()
            {
                placeholder.StartBuild(buildingId);
            }

            public ICommandSerializer Serializer => new CommandSerializer(buildingId, placeholder);
        }

        private class CommandSerializer : ICommandSerializer
        {
            private Id<Building> buildingId;
            private Id<BuildingPlaceholder> placeholder;

            public CommandSerializer(Id<Building> buildingId, BuildingPlaceholder placeholder)
            {
                this.buildingId = buildingId;
                this.placeholder = placeholder.Id;
            }

            // ReSharper disable once UnusedMember.Local
            public CommandSerializer() { }

            public ICommand GetCommand(GameInstance game)
                => new CommandImplementation(buildingId, game.State.Find(placeholder));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref buildingId);
                stream.Serialize(ref placeholder);
            }
        }
    }
}
