using System;
using Bearded.TD.Game.Debug;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game.Generation;

static class TilemapGenerator
{
    public static ILevelGenerator From(LevelGenerationMethod method, Logger logger, LevelDebugMetadata metadata)
    {
        return method switch
        {
            LevelGenerationMethod.Legacy => new DefaultLevelGenerator(logger, metadata),
            LevelGenerationMethod.Perlin => new PerlinLevelGenerator(logger, metadata),
            LevelGenerationMethod.Empty => new EmptyLevelGenerator(),
            LevelGenerationMethod.Semantic => new SemanticLevelGenerator(logger, metadata),
            _ => throw new ArgumentOutOfRangeException(nameof(method), "Unsupported method")
        };
    }
}