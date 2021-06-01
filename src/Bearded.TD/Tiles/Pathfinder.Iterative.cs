using System;
using System.Collections.Generic;
using System.Linq;

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
                AStarPathfinder aStar => new IterativeAStarPathfinder(aStar, AStarPathfinder.State.New(origin, target)),
                _ => throw new NotSupportedException()
            };
        }

        public readonly struct IterativeState
        {
            public IEnumerable<Tile> SeenTiles { get; init; }
            public IEnumerable<Tile> OpenTiles { get; init; }
            public Tile? NextOpenTile { get; init; }
        }

        private sealed class IterativeAStarPathfinder : IIterativePathfinder
        {
            private readonly AStarPathfinder pathfinder;
            private readonly AStarPathfinder.State state;

            public IterativeAStarPathfinder(AStarPathfinder pathfinder, AStarPathfinder.State state)
            {
                this.pathfinder = pathfinder;
                this.state = state;
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
                    NextOpenTile = state.Open.Count == 0 ? null : state.Open.Peek().Value
                };
            }
        }
    }
}
