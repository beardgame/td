using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game
{
    interface IGameSettings
    {
        int LevelSize { get; }
        WorkerDistributionMethod WorkerDistributionMethod { get; }
    }
    
    sealed class GameSettings : IGameSettings
    {
        public int LevelSize { get; }
        public WorkerDistributionMethod WorkerDistributionMethod { get; }

        private GameSettings(IGameSettings builder)
        {
            LevelSize = builder.LevelSize;
            WorkerDistributionMethod = builder.WorkerDistributionMethod;
        }

        public sealed class Builder : IGameSettings
        {
            public int LevelSize { get; set; }
            public WorkerDistributionMethod WorkerDistributionMethod { get; set; }

            public Builder()
            {
                // Initialize default values
                LevelSize = Constants.Game.World.Radius;
                WorkerDistributionMethod = WorkerDistributionMethod.OnePerPlayer;
            }

            public Builder(IGameSettings template)
            {
                // Copy values
                LevelSize = template.LevelSize;
                WorkerDistributionMethod = template.WorkerDistributionMethod;
            }
            
            public GameSettings Build() => new GameSettings(this);
        }

        public class Serializer
        {
            private int levelSize;
            private byte workerDistributionMethod;
            
            public Serializer(IGameSettings gameSettings)
            {
                levelSize = gameSettings.LevelSize;
                workerDistributionMethod = (byte) gameSettings.WorkerDistributionMethod;
            }

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref levelSize);
                stream.Serialize(ref workerDistributionMethod);
            }

            public Builder ToGameSettingsBuilder() => new Builder
            {
                LevelSize = levelSize,
                WorkerDistributionMethod = (WorkerDistributionMethod) workerDistributionMethod,
            };

            public GameSettings ToGameSettings() => ToGameSettingsBuilder().Build();
        }
    }
}
