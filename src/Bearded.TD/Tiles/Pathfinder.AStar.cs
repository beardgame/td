using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bearded.TD.Tiles;

abstract partial class Pathfinder
{
    public static Pathfinder CreateAStar(StepCostFunction costFunction, double minimumCost)
        => new AStarPathfinder(costFunction, minimumCost);

    private sealed class AStarPathfinder : Pathfinder
    {
        internal readonly StepCostFunction CostOfStep;
        internal readonly double MinimumCost;

        public AStarPathfinder(StepCostFunction stepCostFunction, double minimumCost)
        {
            CostOfStep = stepCostFunction;
            MinimumCost = minimumCost;
        }

        public readonly struct State
        {
            public Tile Target { get; private init; }

            public Dictionary<Tile, (double CostFromOrigin, int StepsFromOrigin, Direction StepHere)>
                Seen { get; private init; }

            // key is tentative total cost _through_ tile, implementing A*
            public Bearded.Utilities.Collections.PriorityQueue<double, Tile> Open { get; private init; }

            public static State New(Tile origin, Tile target)
            {
                var state = new State
                {
                    Target = target,
                    Seen = new(origin.DistanceTo(target) * 6)
                    {
                        [origin] = (0, 0, Direction.Unknown)
                    },
                    Open = new()
                };
                state.Open.Enqueue(0, origin);
                return state;
            }
        }

        public override Result? FindPath(Tile origin, Tile target)
        {
            if (origin == target)
                return Result.Empty;

            var state = State.New(origin, target);

            while (TryExpandLowestCostOpenNode(in state))
            {
            }

            return GetResult(state);
        }

        public bool TryExpandLowestCostOpenNode(in State state)
        {
            if (state.Open.Count == 0)
                return false;

            if (state.Open.Peek().Value == state.Target)
                return false;

            var (_, currentTile) = state.Open.Dequeue();

            var (currentCost, currentSteps, _) = state.Seen[currentTile];

            foreach (var direction in Tilemap.Directions)
            {
                var stepCost = CostOfStep(currentTile, direction);
                if (!stepCost.HasValue)
                    continue;

                var neighborTile = currentTile.Neighbor(direction);
                var costToHere = currentCost + stepCost.Value;
                var minimumCostFromHereToTarget = neighborTile.DistanceTo(state.Target) * MinimumCost;

                // This bias makes the algorithm _strictly speaking_ incorrect.
                // However, if there are fewer tiles than what we assume with the constant below, it biases us
                // towards choosing open tiles closer to the target, which will result in exploring fewer options
                // if there are several optimal paths.
                const int maximumNumberOfTilesAssumption = 1_000_000;
                var bias = minimumCostFromHereToTarget * (1.0 / maximumNumberOfTilesAssumption);

                var tentativeTotalCostThroughCurrent = costToHere + minimumCostFromHereToTarget + bias;

                if (!state.Seen.TryGetValue(neighborTile, out var value))
                {
                    state.Seen[neighborTile] = (costToHere, currentSteps + 1, direction);
                    state.Open.Enqueue(tentativeTotalCostThroughCurrent, neighborTile);
                }
                else if (costToHere < value.CostFromOrigin)
                {
                    state.Seen[neighborTile] = (costToHere, currentSteps + 1, direction);
                    state.Open.DecreasePriority(neighborTile, tentativeTotalCostThroughCurrent);
                }
            }

            return true;
        }

        public static Result? GetResult(State state)
        {
            if (!state.Seen.TryGetValue(state.Target, out var targetData))
                return null;

            var (pathCost, pathLength, _) = targetData;
            var pathBuilder = ImmutableArray.CreateBuilder<Direction>(pathLength);
            var currentTile = state.Target;
            for (var i = 0; i < pathLength; i++)
            {
                var stepHere = state.Seen[currentTile].StepHere;
                pathBuilder.Add(stepHere);
                currentTile = currentTile.Neighbor(stepHere.Opposite());
            }
            pathBuilder.Reverse();

            return new Result(pathBuilder.MoveToImmutable(), pathCost);
        }
    }
}