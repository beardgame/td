using System;
using Bearded.TD.Commands;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    static class UpgradeBuilding
    {
        public static IRequest<GameInstance> Request(GameInstance game)
            => new Implementation(game);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly GameInstance game;

            public Implementation(GameInstance game)
            {
                this.game = game;
            }

            public override bool CheckPreconditions() => true;

            public override void Execute()
            {
                throw new NotImplementedException();
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer();
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            // ReSharper disable once UnusedMember.Local
            public Serializer() { }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
            {
                return new Implementation(game);
            }

            public override void Serialize(INetBufferStream stream) { }
        }
    }
}
