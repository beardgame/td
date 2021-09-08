using System;
using System.Linq;
using Bearded.TD.Game.Simulation.World;
using FsCheck;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    public class TilemapGenerators
    {
        public static Arbitrary<TileType> TileTypes() => Arb.From(Gen.Elements(
            Enum.GetValues<TileType>().Except(new[] { TileType.Unknown }).ToArray()
            ));
    }
}
