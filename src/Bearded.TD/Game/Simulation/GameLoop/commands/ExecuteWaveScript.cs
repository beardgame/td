using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.GameLoop;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.GameLoop;

static class ExecuteWaveScript
{
    public static ISerializableCommand<GameInstance> Command(GameState game, WaveScript script, Instant downtimeStart)
        => new Implementation(game, script, downtimeStart);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameState game;
        private readonly WaveScript script;
        private readonly Instant downtimeStart;

        public Implementation(GameState game, WaveScript script, Instant downtimeStart)
        {
            this.game = game;
            this.script = script;
            this.downtimeStart = downtimeStart;
        }

        public void Execute()
        {
            game.WaveDirector.ExecuteScript(script, downtimeStart);
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer =>
            new Serializer(script, downtimeStart);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private readonly WaveScriptSerializer scriptSerializer;
        private double downtimeStart;

        [UsedImplicitly]
        public Serializer()
        {
            scriptSerializer = new WaveScriptSerializer();
        }

        public Serializer(WaveScript script, Instant downtimeStart)
        {
            scriptSerializer = new WaveScriptSerializer(script);
            this.downtimeStart = downtimeStart.NumericValue;
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            => new Implementation(game.State, scriptSerializer.ToWaveScript(game), new Instant(downtimeStart));

        public void Serialize(INetBufferStream stream)
        {
            scriptSerializer.Serialize(stream);
            stream.Serialize(ref downtimeStart);
        }
    }
}
