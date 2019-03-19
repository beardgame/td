using System.Collections.Generic;
using System.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Tiles
{
    sealed class Footprint
    {
        /*
            X
        */
        public static readonly Footprint Single = new Footprint(new[]
        {
            new Step(0, 0)
        });

        /*
            #
           X #
        */
        public static readonly Footprint TriangleUp = new Footprint(new[]
        {
            new Step(0, 0), new Step(Direction.Right), new Step(Direction.UpRight),
        });

        /*
           X #
            #
        */
        public static readonly Footprint TriangleDown = new Footprint(new[]
        {
            new Step(0, 0), new Step(Direction.Right), new Step(Direction.DownRight),
        });

        /*
           # #
          # X #
           # #
        */
        public static readonly Footprint CircleSeven = new Footprint(new[]
        {
            new Step(0, 0),
            new Step(Direction.Left), new Step(Direction.DownLeft), new Step(Direction.DownRight),
            new Step(Direction.Right), new Step(Direction.UpRight), new Step(Direction.UpLeft),
        });
        private readonly IEnumerable<Step> tileOffsets;
        private readonly Difference2 rootTileOffset; // vector that points from center of footprint to center of root tile
        
        public Footprint(IEnumerable<Step> tileOffsets)
        {
            var steps = tileOffsets as IList<Step> ?? tileOffsets.ToList();
            this.tileOffsets = steps;
            rootTileOffset = -steps
                    .Select(step =>
                        step.X * Constants.Game.World.HexagonGridUnitX + step.Y * Constants.Game.World.HexagonGridUnitY)
                    .Aggregate((diff1, diff2) => diff1 + diff2) / steps.Count;
        }

        public IEnumerable<Tile> OccupiedTiles(Tile rootTile)
            => tileOffsets.Select(rootTile.Offset);

        public Position2 Center(Tile rootTile) => Level.GetPosition(rootTile) - rootTileOffset;

        public Tile RootTileClosestToWorldPosition(Position2 position) => Level.GetTile(position + rootTileOffset);
    }
}
