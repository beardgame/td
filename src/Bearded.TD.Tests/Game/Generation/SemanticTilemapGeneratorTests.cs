using Bearded.TD.Game.Debug;
using Bearded.TD.Game.Generation;
using Bearded.Utilities.IO;

namespace Bearded.TD.Tests.Game.Generation
{
    public sealed class SemanticTilemapGeneratorTests : TilemapGeneratorTests
    {
        internal override ITilemapGenerator Generator { get; }

        public SemanticTilemapGeneratorTests()
        {
            Generator = new SemanticTilemapGenerator(new Logger {MirrorToConsole = false}, new LevelDebugMetadata());
        }
    }
}
