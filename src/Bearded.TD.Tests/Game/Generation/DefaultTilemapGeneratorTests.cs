using Bearded.TD.Game.Generation;
using Bearded.TD.Game.World;
using Bearded.Utilities.IO;

namespace Bearded.TD.Tests.Game.Generation
{
    public sealed class DefaultTilemapGeneratorTests : TilemapGeneratorTests
    {
        internal override ITilemapGenerator Generator { get; }

        public DefaultTilemapGeneratorTests()
        {
            Generator = new DefaultTilemapGenerator(new Logger {MirrorToConsole = false}, new LevelDebugMetadata());
        }
    }
}
