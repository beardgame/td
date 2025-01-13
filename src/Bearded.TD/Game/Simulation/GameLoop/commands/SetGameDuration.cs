using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Networking.Serialization;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.GameLoop;

static class SetGameDuration
{
    public static ISerializableCommand<GameInstance> Command(GameState game, int chapterCount, int waveCount)
        => new Implementation(game, chapterCount, waveCount);

    private sealed class Implementation(GameState game, int chapterCount, int waveCount)
        : ISerializableCommand<GameInstance>
    {
        public void Execute()
        {
            game.Progression.SetGameDuration(chapterCount, waveCount);
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer =>
            new Serializer(chapterCount, waveCount);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private int chapterCount;
        private int waveCount;

        [UsedImplicitly]
        public Serializer() { }

        public Serializer(int chapterCount, int waveCount)
        {
            this.chapterCount = chapterCount;
            this.waveCount = waveCount;
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            => new Implementation(game.State, chapterCount, waveCount);

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref chapterCount);
            stream.Serialize(ref waveCount);
        }
    }
}
