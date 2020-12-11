using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities.Collections;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Loading
{
    static class SetModEnabled
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, ModMetadata mod, bool enabled)
            => new Implementation(game, mod, enabled);

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;
            private readonly ModMetadata mod;
            private readonly bool enabled;

            public Implementation(GameInstance game, ModMetadata mod, bool enabled)
            {
                this.game = game;
                this.mod = mod;
                this.enabled = enabled;
            }

            public void Execute()
            {
                if (enabled)
                {
                    game.ContentManager.EnableMod(mod);
                    game.ChatLog.Add(new ChatMessage(null, $"Mod enabled: {mod.Name}"));
                }
                else
                {
                    game.ContentManager.DisableMod(mod);
                    game.ChatLog.Add(new ChatMessage(null, $"Mod disabled: {mod.Name}"));
                }
                game.Players.Where(p => p.ConnectionState == PlayerConnectionState.Ready)
                    .ForEach(p => p.ConnectionState = PlayerConnectionState.Waiting);
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(mod, enabled);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private string mod;
            private bool enabled;

            [UsedImplicitly]
            public Serializer() {}

            public Serializer(ModMetadata mod, bool enabled)
            {
                this.enabled = enabled;
                this.mod = mod.Id;
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game, game.ContentManager.FindMod(mod), enabled);

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref mod);
                stream.Serialize(ref enabled);
            }
        }
    }
}

