using Bearded.TD.Tiles;
using FsCheck;
using FsCheck.Fluent;
using JetBrains.Annotations;

namespace Bearded.TD.Tests.Game.Generation
{
    public sealed class TileGenerator
    {
        [UsedImplicitly]
        public static Arbitrary<Tile> Tile() => Arb.From(
            ArbMap.Default.GeneratorFor<int>()
                .Two()
                .Select(tuple => new Tile(tuple.Item1, tuple.Item2)));
    }
}
