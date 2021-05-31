using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Tiles
{
    abstract class Pathfinder
    {
        public sealed record Result(ImmutableArray<Direction> Path, double Cost)
        {
            public static Result Empty { get; } = new(ImmutableArray<Direction>.Empty, 0);
        }

        // these should return null if the step is illegal
        public delegate double? TileCostFunction(Tile tile);

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



        private sealed class AStarPathfinder : Pathfinder
        {
            internal readonly StepCostFunction CostOfStep;
            internal readonly double MinimumCost;

            public AStarPathfinder(StepCostFunction stepCostFunction, double minimumCost)
            {
                CostOfStep = stepCostFunction;
                MinimumCost = minimumCost;
            }

            public override Result? FindPath(Tile origin, Tile target)
            {
                if (origin == target)
                    return Result.Empty;

                var seenTiles = new Dictionary<Tile, (double CostFromOrigin, int StepsFromOrigin, Direction StepHere)>
                    (origin.DistanceTo(target) * 6)
                    {
                        [origin] = (0, 0, Direction.Unknown)
                    };

                // key is tentative total cost through tile, implementing A*
                var queue = new PriorityQueue<double, Tile>();
                queue.Enqueue(0, origin);

                while (true)
                {
                    var (_, currentTile) = queue.Dequeue();
                    if (currentTile == target)
                        break;

                    var (currentCost, currentSteps, _) = seenTiles[currentTile];

                    foreach (var direction in Tilemap.Directions)
                    {
                        var stepCost = CostOfStep(currentTile, direction);
                        if (!stepCost.HasValue)
                            break;

                        var neighborTile = currentTile.Neighbour(direction);
                        var costToHere = currentCost + stepCost.Value;
                        var minimumCostFromHereToTarget = neighborTile.DistanceTo(target) * MinimumCost;
                        var tentativeTotalCostThroughCurrent = costToHere + minimumCostFromHereToTarget;

                        if (seenTiles.TryGetValue(neighborTile, out var value))
                        {
                            var (previousCostToHere, _, _) = value;
                            if (costToHere < previousCostToHere)
                            {
                                seenTiles[neighborTile] = (costToHere, currentSteps + 1, direction);
                                queue.DecreasePriority(neighborTile, tentativeTotalCostThroughCurrent);
                            }
                        }
                        else
                        {
                            seenTiles[neighborTile] = (costToHere, currentSteps + 1, direction);
                            queue.Enqueue(tentativeTotalCostThroughCurrent, neighborTile);
                        }
                    }
                }

                return buildResult(target, seenTiles);
            }

            private static Result buildResult(
                Tile target,
                Dictionary<Tile, (double CostFromOrigin, int StepsFromOrigin, Direction StepHere)> seenTiles)
            {
                var (pathCost, pathLength, _) = seenTiles[target];
                var pathBuilder = ImmutableArray.CreateBuilder<Direction>(pathLength);
                var currentTile = target;
                for (var i = 0; i < pathLength; i++)
                {
                    var stepHere = seenTiles[currentTile].StepHere;
                    pathBuilder.Add(stepHere);
                    currentTile = currentTile.Neighbour(stepHere.Opposite());
                }

                pathBuilder.Reverse();
                return new Result(pathBuilder.MoveToImmutable(), pathCost);
            }
        }
    }
}
