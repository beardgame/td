using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Buildings;

interface IBuildBuildingPrecondition
{
    public readonly record struct Parameters(GameState Game, PositionedFootprint Footprint);

    public readonly record struct Result(
        bool IsValid,
        ImmutableArray<Tile> BadTiles,
        ImmutableArray<TileEdge> BadEdges,
        Resource<Scrap> Cost)
    {
        public static Result Valid => FromBool(true);
        public static Result Invalid => FromBool(false);

        public static Result FromBool(bool isValid)
            => new(isValid, ImmutableArray<Tile>.Empty, ImmutableArray<TileEdge>.Empty, Resource<Scrap>.Zero);

        public static Result FromCost(Resource<Scrap> cost)
            => new(true, ImmutableArray<Tile>.Empty, ImmutableArray<TileEdge>.Empty, cost);

        public static Result And(Result one, Result other) => new(
            one.IsValid && other.IsValid,
            merge(one.BadTiles, other.BadTiles),
            merge(one.BadEdges, other.BadEdges),
            one.Cost + other.Cost);

        private static ImmutableArray<T> merge<T>(ImmutableArray<T> one, ImmutableArray<T> other)
        {
            return (one.Length, other.Length) switch
            {
                (_, 0) => one,
                (0, _) => other,
                _ => one.Concat(other).Distinct().ToImmutableArray()
            };
        }
    }

    Result CanBuild(Parameters parameters);
}
