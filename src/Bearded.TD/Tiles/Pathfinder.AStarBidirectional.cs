using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bearded.TD.Tiles
{
    abstract partial class Pathfinder
    {
        public static Pathfinder CreateBidirectionalAStar(StepCostFunction costFunction, double minimumCost)
            => new AStarBidirectionalPathfinder(costFunction, minimumCost);

        private sealed class AStarBidirectionalPathfinder : Pathfinder
        {
            internal readonly AStarPathfinder AStar;

            public AStarBidirectionalPathfinder(StepCostFunction costFunction, double minimumCost)
            {
                // TODO: we currently assume that step costs are symmetrical, this might not always be valid
                // if it isn't valid, we'd have to use two different pathfinders and invert the cost function for one
                AStar = new AStarPathfinder(costFunction, minimumCost);
            }

            public override Result? FindPath(Tile origin, Tile target)
            {
                if (origin == target)
                    return Result.Empty;

                var stateForward = AStarPathfinder.State.New(origin, target);
                var stateBackward = AStarPathfinder.State.New(target, origin);
                var tilesExpanded = new HashSet<Tile>();
                Tile? tileReachedByBothSearches = null;

                while (
                    TryExpand(stateForward, tilesExpanded, ref tileReachedByBothSearches) &&
                    TryExpand(stateBackward, tilesExpanded, ref tileReachedByBothSearches))
                {
                }

                return GetResult(stateForward, stateBackward, tileReachedByBothSearches);
            }

            public bool TryExpand(in AStarPathfinder.State state, HashSet<Tile> tilesExpanded, ref Tile? pathFound)
            {
                if (state.Open.Count == 0)
                    return false;

                var expandingTile = state.Open.Peek().Value;

                if (tilesExpanded.Add(expandingTile))
                {
                    return AStar.TryExpandLowestCostOpenNode(state);
                }

                pathFound = expandingTile;
                return false;

            }

            public static Result? GetResult(
                in AStarPathfinder.State stateForward,
                in AStarPathfinder.State stateBackward,
                Tile? possibleConnectingTile)
            {
                if (possibleConnectingTile is not { } connectingTile)
                    return Result.Empty;

                var (forwardPathCost, forwardPathLength, _) = stateForward.Seen[connectingTile];
                var (backwardPathCost, backwardPathLength, _) = stateBackward.Seen[connectingTile];
                var pathCost = forwardPathCost + backwardPathCost;
                var pathLength = forwardPathLength + backwardPathLength;

                var pathBuilder = ImmutableArray.CreateBuilder<Direction>(pathLength);
                for (var i = 0; i < pathLength; i++)
                    pathBuilder.Add(Direction.Unknown);

                var currentTile = connectingTile;
                for (var i = 0; i < forwardPathLength; i++)
                {
                    var stepHere = stateForward.Seen[currentTile].StepHere;
                    pathBuilder[forwardPathLength - i - 1] = stepHere;
                    currentTile = currentTile.Neighbour(stepHere.Opposite());
                }
                currentTile = connectingTile;
                for (var i = 0; i < backwardPathLength; i++)
                {
                    var stepHere = stateBackward.Seen[currentTile].StepHere.Opposite();
                    pathBuilder[forwardPathLength + i] = stepHere;
                    currentTile = currentTile.Neighbour(stepHere);
                }

                return new Result(pathBuilder.MoveToImmutable(), pathCost);
            }
        }
    }
}
