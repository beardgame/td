using Bearded.TD.Game.Debug;
using Bearded.TD.Game.Generation;
using Bearded.Utilities.IO;

namespace Bearded.TD.Tests.Game.Generation
{
    public sealed class DefaultTilemapGeneratorTests : TilemapGeneratorTests
    {
        internal override ILevelGenerator Generator { get; }

        public DefaultTilemapGeneratorTests()
        {
            Generator = new DefaultLevelGenerator(new Logger {MirrorToConsole = false}, new LevelDebugMetadata());
        }
    }
}
