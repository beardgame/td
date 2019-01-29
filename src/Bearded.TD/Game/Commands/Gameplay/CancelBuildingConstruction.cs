using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class CancelBuildingConstruction
    {
        public static IRequest<GameInstance> Request(BuildingPlaceholder placeholder)
            => new Implementation(placeholder);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly BuildingPlaceholder placeholder;

            public Implementation(BuildingPlaceholder placeholder)
            {
                this.placeholder = placeholder;
            }

            public override bool CheckPreconditions() => true;

            public override void Execute() => placeholder.CancelBuild();

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(placeholder);
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<BuildingPlaceholder> placeholder;

            // ReSharper disable once UnusedMember.Local
            public Serializer() { }

            public Serializer(BuildingPlaceholder placeholder)
            {
                this.placeholder = placeholder.Id;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
            {
                return new Implementation(game.State.Find(placeholder));
            }

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref placeholder);
            }
        }
    }
}
