using System;
using System.Linq;
using Bearded.TD.Game.Simulation.World;
using FsCheck;
using FsCheck.Fluent;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    public sealed class TilemapGenerators
    {
        public static Arbitrary<TileType> TileTypes() => Arb.From(Gen.Elements(
            Enum.GetValues<TileType>().Except(new[] { TileType.Unknown }).ToArray()
            ));
    }
}
