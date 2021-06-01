using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Tiles
{
    interface IIterativePathfinder
    {
        bool TryAdvanceStep();
        Pathfinder.Result? GetResult();

        Pathfinder.IterativeState GetCurrentState();
    }

    abstract partial class Pathfinder
    {
        public IIterativePathfinder FindPathIteratively(Tile origin, Tile target)
        {
            return this switch
            {
                AStarPathfinder aStar => new IterativeAStarPathfinder(aStar, origin, target),
                AStarBidirectionalPathfinder aStarBi =>
                    new IterativeAStarBidirectionalPathfinder(aStarBi, origin, target),
                _ => throw new NotSupportedException()
            };
        }

        public readonly struct IterativeState
        {
            public IEnumerable<Tile> SeenTiles { get; init; }
            public IEnumerable<Tile> OpenTiles { get; init; }
            public IEnumerable<Tile> NextOpenTiles { get; init; }
        }

        private sealed class IterativeAStarPathfinder : IIterativePathfinder
        {
            private readonly AStarPathfinder pathfinder;
            private readonly AStarPathfinder.State state;

            public IterativeAStarPathfinder(AStarPathfinder pathfinder, Tile origin, Tile target)
            {
                this.pathfinder = pathfinder;
                state = AStarPathfinder.State.New(origin, target);
            }

            public bool TryAdvanceStep()
            {
                return pathfinder.TryExpandLowestCostOpenNode(in state);
            }

            public Result? GetResult()
            {
                return AStarPathfinder.GetResult(state);
            }

            public IterativeState GetCurrentState()
            {
                return new()
                {
                    SeenTiles = state.Seen.Keys,
                    OpenTiles = state.Open.Select(kvp => kvp.Value),
                    NextOpenTiles = state.Open.Count == 0 ? Enumerable.Empty<Tile>() : state.Open.Peek().Value.Yield()
                };
            }
        }

        private sealed class IterativeAStarBidirectionalPathfinder : IIterativePathfinder
        {
            private readonly AStarBidirectionalPathfinder pathfinder;
            private readonly AStarPathfinder.State stateForward;
            private readonly AStarPathfinder.State stateBackward;
            private readonly HashSet<Tile> tilesExpanded;
            private Tile? tileReachedByBothSearches;
            private bool expanding = true;

            public IterativeAStarBidirectionalPathfinder(AStarBidirectionalPathfinder pathfinder, Tile origin,
                Tile target)
            {
                this.pathfinder = pathfinder;
                stateForward = AStarPathfinder.State.New(origin, target);
                stateBackward = AStarPathfinder.State.New(target, origin);
                tilesExpanded = new HashSet<Tile>();
                tileReachedByBothSearches = null;
            }

            public bool TryAdvanceStep()
            {
                if (!expanding)
                    return false;

                return expanding =
                    pathfinder.TryExpand(stateForward, tilesExpanded, ref tileReachedByBothSearches) &&
                    pathfinder.TryExpand(stateBackward, tilesExpanded, ref tileReachedByBothSearches);
            }

            public Result? GetResult()
            {
                return AStarBidirectionalPathfinder.GetResult(stateForward, stateBackward, tileReachedByBothSearches);
            }

            public IterativeState GetCurrentState()
            {
                var openTiles = new List<Tile>();
                if (stateForward.Open.Count > 0)
                    openTiles.Add(stateForward.Open.Peek().Value);
                if (stateBackward.Open.Count > 0)
                    openTiles.Add(stateBackward.Open.Peek().Value);

                return new()
                {
                    SeenTiles = stateForward.Seen.Keys.Concat(stateBackward.Seen.Keys),
                    OpenTiles = stateForward.Open.Concat(stateBackward.Open).Select(kvp => kvp.Value),
                    NextOpenTiles = openTiles
                };
            }
        }
    }
}
