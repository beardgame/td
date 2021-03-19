using System;
using Bearded.TD.Game.Debug;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game.Generation
{
    static class TilemapGenerator
    {
        public static ITilemapGenerator From(LevelGenerationMethod method, Logger logger, LevelDebugMetadata metadata)
        {
            return method switch
            {
                LevelGenerationMethod.Legacy => new DefaultTilemapGenerator(logger, metadata),
                LevelGenerationMethod.Perlin => new PerlinTilemapGenerator(logger, metadata),
                LevelGenerationMethod.Empty => new EmptyTilemapGenerator(),
                LevelGenerationMethod.Semantic => new SemanticTilemapGenerator(logger, metadata),
                _ => throw new ArgumentOutOfRangeException(nameof(method), "Unsupported method")
            };
        }
    }
}
