using System;

namespace Bearded.TD.Game
{
    [Serializable]
    sealed class GameSettings
    {
        public int LevelSize { get; }

        private GameSettings(Builder builder)
        {
            LevelSize = builder.LevelSize;
        }

        public sealed class Builder
        {
            public int LevelSize { get; set; }

            public Builder()
            {
                // Initialize default values
                LevelSize = Constants.Game.World.Radius;
            }

            public Builder(Builder template)
            {
                // Copy values from an existing builder
                LevelSize = template.LevelSize;
            }
            
            public GameSettings Build() => new GameSettings(this);
        }
    }
}
