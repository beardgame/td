using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Loading;

static class SetEnabledMods
{
    public static ISerializableCommand<GameInstance> Command(GameInstance game, IEnumerable<ModMetadata> mods)
        => new Implementation(game, mods);

    private sealed class Implementation : ISerializableCommand<GameInstance>
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
            DebugAssert.State.Satisfies(game.Me.ConnectionState == PlayerConnectionState.Connecting);
            game.ContentManager.SetEnabledMods(mods);
            game.Players.ForEach(p => p.ConnectionState = PlayerConnectionState.LoadingMods);
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(mods);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private string[] mods = {};

        public Serializer(IEnumerable<ModMetadata> mods)
        {
            this.mods = mods.Select(m => m.Id).ToArray();
        }

        [UsedImplicitly]
        public Serializer() {}

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