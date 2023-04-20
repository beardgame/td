using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Generation;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game;

interface IGameSettings
{
    int Seed { get; }
    public IReadOnlyList<string> ActiveModIds { get; }
    ModAwareId? GameMode { get; }
    int LevelSize { get; }
    LevelGenerationMethod LevelGenerationMethod { get; }
}

sealed class GameSettings : IGameSettings
{
    public int Seed { get; }
    public IReadOnlyList<string> ActiveModIds { get; }
    public ModAwareId? GameMode { get; }
    public int LevelSize { get; }
    public LevelGenerationMethod LevelGenerationMethod { get; }

    private GameSettings(IGameSettings builder)
    {
        Seed = builder.Seed;
        ActiveModIds = builder.ActiveModIds.ToImmutableArray();
        GameMode = builder.GameMode;
        LevelSize = builder.LevelSize;
        LevelGenerationMethod = builder.LevelGenerationMethod;
    }

    public sealed class Builder : IGameSettings
    {
        public int Seed { get; set; }
        public List<string> ActiveModIds { get; } = new();
        IReadOnlyList<string> IGameSettings.ActiveModIds => ActiveModIds;
        public ModAwareId? GameMode { get; set; }
        public int LevelSize { get; set; } = 32;

        public LevelGenerationMethod LevelGenerationMethod { get; set; }
            = LevelGenerationMethod.Default;

        public Builder() { }

        public Builder(IGameSettings template)
        {
            // Copy values
            Seed = template.Seed;
            ActiveModIds.AddRange(template.ActiveModIds);
            GameMode = template.GameMode is { IsValid: true } ? template.GameMode : null;
            LevelSize = template.LevelSize;
            LevelGenerationMethod = template.LevelGenerationMethod;
        }

        public GameSettings Build() => new(this);
    }

    public sealed class Serializer
    {
        private int seed;
        private string[] activeModIds;
        private ModAwareId gameMode;
        private int levelSize;
        private byte levelGenerationMethod;

        public Serializer(IGameSettings gameSettings)
        {
            seed = gameSettings.Seed;
            activeModIds = gameSettings.ActiveModIds.ToArray();
            gameMode = gameSettings.GameMode ?? ModAwareId.Invalid;
            levelSize = gameSettings.LevelSize;
            levelGenerationMethod = (byte) gameSettings.LevelGenerationMethod;
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref seed);
            stream.SerializeArrayCount(ref activeModIds);
            for (var i = 0; i < activeModIds.Length; i++)
            {
                stream.Serialize(ref activeModIds[i]);
            }
            stream.Serialize(ref gameMode);
            stream.Serialize(ref levelSize);
            stream.Serialize(ref levelGenerationMethod);
        }

        public Builder ToGameSettingsBuilder()
        {
            var b = new Builder
            {
                Seed = seed,
                GameMode = gameMode.IsValid ? gameMode : null,
                LevelSize = levelSize,
                LevelGenerationMethod = (LevelGenerationMethod)levelGenerationMethod,
            };
            b.ActiveModIds.AddRange(activeModIds);
            return b;
        }

        public GameSettings ToGameSettings() => ToGameSettingsBuilder().Build();
    }
}
