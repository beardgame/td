using System;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    static class SetModEnabled
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, ModMetadata mod, bool enabled)
            => new Implementation(game, mod, enabled);

        private class Implementation : ISerializableCommand<GameInstance>
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
                }
                else
                {
                    game.ContentManager.DisableMod(mod);
                }
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(mod, enabled);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private string mod;
            private bool enabled;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

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

