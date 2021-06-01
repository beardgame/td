using System;
using System.Collections.Immutable;

namespace Bearded.TD.Tiles
{
    abstract partial class Pathfinder
    {
        public sealed record Result(ImmutableArray<Direction> Path, double Cost)
        {
            public static Result Empty { get; } = new(ImmutableArray<Direction>.Empty, 0);
        }

        /// <summary>
        /// Cost functions must:
        /// - return `null` if the tile is illegal
        /// - never return negative values
        /// </summary>
        public delegate double? TileCostFunction(Tile tile);

        /// <summary>
        /// Cost functions must:
        /// - return `null` if the step is illegal
        /// - never return negative values
        /// </summary>
        public delegate double? StepCostFunction(Tile tile, Direction direction);

        public static Pathfinder Default { get; } = new AStarPathfinder((_, _) => 1, 1);

        public static Pathfinder WithTileCosts(TileCostFunction costFunction, double minimumCost) =>
            WithStepCosts((tile, direction) => costFunction(tile.Neighbour(direction)), minimumCost);

        public static Pathfinder WithStepCosts(StepCostFunction costFunction, double minimumCost)
        {
            if (minimumCost <= 0)
                throw new ArgumentOutOfRangeException(nameof(minimumCost), "Minimum cost must be positive.");

            return new AStarPathfinder(costFunction, minimumCost);
        }

        private Pathfinder()
        {
        }

        public abstract Result? FindPath(Tile origin, Tile target);

        public Pathfinder InArea(IArea area)
        {
            return this switch
            {
                AStarPathfinder aStar => new AStarPathfinder(constrainArea(aStar.CostOfStep, area), aStar.MinimumCost),
                _ => throw new NotSupportedException()
            };
        }

        private static StepCostFunction constrainArea(StepCostFunction function, IArea area)
            => (tile, direction) => area.Contains(tile.Neighbour(direction)) ? function(tile, direction) : null;
    }
}
