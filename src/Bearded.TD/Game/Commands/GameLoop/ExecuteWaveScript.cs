using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.GameLoop;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Networking.Serialization;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.GameLoop
{
    static class ExecuteWaveScript
    {
        public static ISerializableCommand<GameInstance> Command(GameState game, WaveScript script)
            => new Implementation(game, script);

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameState game;
            private readonly WaveScript script;

            public Implementation(GameState game, WaveScript script)
            {
                this.game = game;
                this.script = script;
            }

            public void Execute()
            {
                game.WaveDirector.ExecuteScript(script);
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(script);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private readonly WaveScriptSerializer scriptSerializer;

            [UsedImplicitly]
            public Serializer()
            {
                scriptSerializer = new WaveScriptSerializer();
            }

            public Serializer(WaveScript script)
            {
                scriptSerializer = new WaveScriptSerializer(script);
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game.State, scriptSerializer.ToWaveScript(game));

            public void Serialize(INetBufferStream stream)
            {
                scriptSerializer.Serialize(stream);
            }
        }
    }
}

