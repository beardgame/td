using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game
{
    sealed class GameSettings
    {
        public int LevelSize { get; }
        public WorkerDistributionMethod WorkerDistributionMethod { get; }

        private GameSettings(Builder builder)
        {
            LevelSize = builder.LevelSize;
            WorkerDistributionMethod = builder.WorkerDistributionMethod;
        }

        public sealed class Builder
        {
            public int LevelSize { get; set; }
            public WorkerDistributionMethod WorkerDistributionMethod { get; set; }

            public Builder()
            {
                // Initialize default values
                LevelSize = Constants.Game.World.Radius;
                WorkerDistributionMethod = WorkerDistributionMethod.OnePerPlayer;
            }

            public Builder(Builder template)
            {
                // Copy values from an existing builder
                LevelSize = template.LevelSize;
                WorkerDistributionMethod = template.WorkerDistributionMethod;
            }
            
            public GameSettings Build() => new GameSettings(this);
        }

        public class Serializer
        {
            private int levelSize;
            private byte workerDistributionMethod;
            
            public Serializer(GameSettings gameSettings)
            {
                levelSize = gameSettings.LevelSize;
                workerDistributionMethod = (byte) gameSettings.WorkerDistributionMethod;
            }

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref levelSize);
                stream.Serialize(ref workerDistributionMethod);
            }

            public GameSettings ToGameSettings()
            {
                return new Builder
                {
                    LevelSize = levelSize,
                    WorkerDistributionMethod = (WorkerDistributionMethod) workerDistributionMethod,
                }.Build();
            }
        }
    }
}
