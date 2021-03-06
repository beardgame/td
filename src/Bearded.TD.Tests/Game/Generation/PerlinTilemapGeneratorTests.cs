using Bearded.TD.Game.Debug;
using Bearded.TD.Game.Generation;
using Bearded.Utilities.IO;

namespace Bearded.TD.Tests.Game.Generation
{
    public sealed class PerlinTilemapGeneratorTests : TilemapGeneratorTests
    {
        internal override ILevelGenerator Generator { get; }

        public PerlinTilemapGeneratorTests()
        {
            Generator = new PerlinLevelGenerator(new Logger {MirrorToConsole = false}, new LevelDebugMetadata());
        }
    }
}
