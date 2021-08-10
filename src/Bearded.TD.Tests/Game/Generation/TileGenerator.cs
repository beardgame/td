using Bearded.TD.Tiles;
using FsCheck;

namespace Bearded.TD.Tests.Game.Generation
{
    public sealed class TileGenerator
    {
        public static Arbitrary<Tile> Tile()
            => Arb.From(
                from x in Arb.Generate<int>()
                from y in Arb.Generate<int>()
                select new Tile(x, y)
            );
    }
}
