using Bearded.TD.Game.Generation;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game
{
    interface IGameSettings
    {
        int Seed { get; }
        int LevelSize { get; }
        WorkerDistributionMethod WorkerDistributionMethod { get; }
        LevelGenerationMethod LevelGenerationMethod { get; }
    }

    sealed class GameSettings : IGameSettings
    {
        public int Seed { get; }
        public int LevelSize { get; }
        public WorkerDistributionMethod WorkerDistributionMethod { get; }
        public LevelGenerationMethod LevelGenerationMethod { get; }

        private GameSettings(IGameSettings builder)
        {
            Seed = builder.Seed;
            LevelSize = builder.LevelSize;
            WorkerDistributionMethod = builder.WorkerDistributionMethod;
            LevelGenerationMethod = builder.LevelGenerationMethod;
        }

        public sealed class Builder : IGameSettings
        {
            public int Seed { get; set; }
            public int LevelSize { get; set; }
            public WorkerDistributionMethod WorkerDistributionMethod { get; set; }
            public LevelGenerationMethod LevelGenerationMethod { get; set; }

            public Builder()
            {
                // Initialize default values
                Seed = StaticRandom.Int();
                LevelSize = Constants.Game.World.Radius;
                WorkerDistributionMethod = WorkerDistributionMethod.Neutral;
                LevelGenerationMethod = LevelGenerationMethod.Default;
            }

            public Builder(IGameSettings template, bool includeRandomAttributes = false)
            {
                // Copy values
                Seed = includeRandomAttributes ? template.Seed : StaticRandom.Int();
                LevelSize = template.LevelSize;
                WorkerDistributionMethod = template.WorkerDistributionMethod;
                LevelGenerationMethod = template.LevelGenerationMethod;
            }

            public GameSettings Build() => new GameSettings(this);
        }

        public class Serializer
        {
            private int seed;
            private int levelSize;
            private byte workerDistributionMethod;
            private byte levelGenerationMethod;

            public Serializer(IGameSettings gameSettings)
            {
                seed = gameSettings.Seed;
                levelSize = gameSettings.LevelSize;
                workerDistributionMethod = (byte) gameSettings.WorkerDistributionMethod;
                levelGenerationMethod = (byte) gameSettings.LevelGenerationMethod;
            }

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref seed);
                stream.Serialize(ref levelSize);
                stream.Serialize(ref workerDistributionMethod);
                stream.Serialize(ref levelGenerationMethod);
            }

            public Builder ToGameSettingsBuilder() => new Builder
            {
                Seed = seed,
                LevelSize = levelSize,
                WorkerDistributionMethod = (WorkerDistributionMethod) workerDistributionMethod,
                LevelGenerationMethod = (LevelGenerationMethod) levelGenerationMethod,
            };

            public GameSettings ToGameSettings() => ToGameSettingsBuilder().Build();
        }
    }
}
