using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.GameLoop;
using Bearded.TD.Networking.Serialization;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.GameLoop;

static class ExecuteWave
{
    public static ISerializableCommand<GameInstance> Command(GameState game, Wave wave) =>
        new Implementation(game, wave);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameState game;
        private readonly Wave wave;

        public Implementation(GameState game, Wave wave)
        {
            this.game = game;
            this.wave = wave;
        }

        public void Execute()
        {
            game.WaveDirector.ExecuteWave(wave);
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer =>
            new Serializer(wave);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private readonly WaveSerializer waveSerializer;

        [UsedImplicitly]
        public Serializer()
        {
            waveSerializer = new WaveSerializer();
        }

        public Serializer(Wave wave)
        {
            waveSerializer = new WaveSerializer(wave);
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game) =>
            new Implementation(game.State, waveSerializer.ToWave(game));

        public void Serialize(INetBufferStream stream)
        {
            waveSerializer.Serialize(stream);
        }
    }
}
