using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Generation;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game
{
    interface IGameSettings
    {
        int Seed { get; }
        ModAwareId? GameMode { get; }
        int LevelSize { get; }
        WorkerDistributionMethod WorkerDistributionMethod { get; }
        LevelGenerationMethod LevelGenerationMethod { get; }
    }

    sealed class GameSettings : IGameSettings
    {
        public int Seed { get; }
        public ModAwareId? GameMode { get; }
        public int LevelSize { get; }
        public WorkerDistributionMethod WorkerDistributionMethod { get; }
        public LevelGenerationMethod LevelGenerationMethod { get; }

        private GameSettings(IGameSettings builder)
        {
            Seed = builder.Seed;
            GameMode = builder.GameMode;
            LevelSize = builder.LevelSize;
            WorkerDistributionMethod = builder.WorkerDistributionMethod;
            LevelGenerationMethod = builder.LevelGenerationMethod;
        }

        public sealed class Builder : IGameSettings
        {
            public int Seed { get; set; }
            public ModAwareId? GameMode { get; set; }
            public int LevelSize { get; set; } = 32;

            public WorkerDistributionMethod WorkerDistributionMethod { get; set; } =
                WorkerDistributionMethod.RoundRobin;

            public LevelGenerationMethod LevelGenerationMethod { get; set; }
                = LevelGenerationMethod.Default;

            public Builder() { }

            public Builder(IGameSettings template)
            {
                // Copy values
                Seed = template.Seed;
                GameMode = template.GameMode;
                LevelSize = template.LevelSize;
                WorkerDistributionMethod = template.WorkerDistributionMethod;
                LevelGenerationMethod = template.LevelGenerationMethod;
            }

            public GameSettings Build() => new(this);
        }

        public sealed class Serializer
        {
            private int seed;
            private ModAwareId gameMode;
            private int levelSize;
            private byte workerDistributionMethod;
            private byte levelGenerationMethod;

            public Serializer(IGameSettings gameSettings)
            {
                seed = gameSettings.Seed;
                gameMode = gameSettings.GameMode ?? ModAwareId.Invalid;
                levelSize = gameSettings.LevelSize;
                workerDistributionMethod = (byte) gameSettings.WorkerDistributionMethod;
                levelGenerationMethod = (byte) gameSettings.LevelGenerationMethod;
            }

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref seed);
                stream.Serialize(ref gameMode);
                stream.Serialize(ref levelSize);
                stream.Serialize(ref workerDistributionMethod);
                stream.Serialize(ref levelGenerationMethod);
            }

            public Builder ToGameSettingsBuilder() => new()
            {
                Seed = seed,
                GameMode = gameMode.IsValid ? gameMode : null,
                LevelSize = levelSize,
                WorkerDistributionMethod = (WorkerDistributionMethod) workerDistributionMethod,
                LevelGenerationMethod = (LevelGenerationMethod) levelGenerationMethod,
            };

            public GameSettings ToGameSettings() => ToGameSettingsBuilder().Build();
        }
    }
}
