using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Workers;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class CancelMiningTask
    {
        public static IRequest<GameInstance> Request(MiningTaskPlaceholder miningTaskPlaceholder)
            => new Implementation(miningTaskPlaceholder);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly MiningTaskPlaceholder miningTaskPlaceholder;

            public Implementation(MiningTaskPlaceholder miningTaskPlaceholder)
            {
                this.miningTaskPlaceholder = miningTaskPlaceholder;
            }

            public override bool CheckPreconditions() => true;

            public override void Execute()
            {
                miningTaskPlaceholder.Cancel();
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer();
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<MiningTaskPlaceholder> miningTaskPlaceholder;

            public Serializer(MiningTaskPlaceholder miningTaskPlaceholder)
            {
                this.miningTaskPlaceholder = miningTaskPlaceholder.Id;
            }

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
            {
                return new Implementation(game.State.Find(miningTaskPlaceholder));
            }

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref miningTaskPlaceholder);
            }
        }
    }
}
