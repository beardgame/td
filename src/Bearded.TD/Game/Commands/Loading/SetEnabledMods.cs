using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class SetEnabledMods
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, IEnumerable<ModMetadata> mods)
            => new Implementation(game, mods);

        private class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;
            private readonly IEnumerable<ModMetadata> mods;

            public Implementation(GameInstance game, IEnumerable<ModMetadata> mods)
            {
                this.game = game;
                this.mods = mods;
            }

            public void Execute()
            {
                DebugAssert.State.Satisfies(game.Me.ConnectionState == PlayerConnectionState.Waiting);
                game.ContentManager.SetEnabledMods(mods);
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer();
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private string[] mods;

            public Serializer(IEnumerable<ModMetadata> mods)
            {
                this.mods = mods.Select(m => m.Id).ToArray();
            }

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game, mods.Select(game.ContentManager.FindMod).ToList());

            public void Serialize(INetBufferStream stream)
            {
                stream.SerializeArrayCount(ref mods);
                for (var i = 0; i < mods.Length; i++)
                {
                    stream.Serialize(ref mods[i]);
                }
            }
        }
    }
}

