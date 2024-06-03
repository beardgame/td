using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.GameLoop;
using Bearded.TD.Game.Simulation.Statistics.Serialization;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Statistics.commands;

static class OpenWaveReport
{
    public static ISerializableCommand<GameInstance> Command(GameState game, Id<Wave> wave, WaveReport waveReport)
        => new Implementation(game, wave, waveReport);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameState game;
        private readonly Id<Wave> wave;
        private readonly WaveReport waveReport;

        public Implementation(GameState game, Id<Wave> wave, WaveReport waveReport)
        {
            this.game = game;
            this.wave = wave;
            this.waveReport = waveReport;
        }

        public void Execute()
        {
            game.Meta.Events.Send(new WaveReportCreated(wave, waveReport));
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer =>
            new Serializer(wave, waveReport);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<Wave> wave = Id<Wave>.Invalid;
        private readonly SerializableWaveReport waveReport = new();

        [UsedImplicitly]
        public Serializer() {}

        public Serializer(Id<Wave> wave, WaveReport waveReport)
        {
            this.wave = wave;
            this.waveReport = new SerializableWaveReport(waveReport);
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            => new Implementation(game.State, wave, waveReport.ToInstance(game));

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref wave);
            waveReport.Serialize(stream);
        }
    }
}
