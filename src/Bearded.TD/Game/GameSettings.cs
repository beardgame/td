using System;

namespace Bearded.TD.Game
{
    [Serializable]
    sealed class GameSettings
    {
        public int LevelSize { get; }
        public WorkerDistributionMethod WorkerDistributionMethod { get; }

        private GameSettings(Builder builder)
        {
            LevelSize = builder.LevelSize;
            WorkerDistributionMethod = builder.WorkerDistributionMethod;
        }

        [Serializable]
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
    }
}
