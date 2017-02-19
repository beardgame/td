using System;
using Bearded.TD.Game.World;
using Bearded.Utilities.Math;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Tiles
{
    abstract class TileSelection
    {
        public static TileSelection Single => FromFootprint(Footprint.Single);
        public static TileSelection Triangle = new TriangleSelection();

        public static TileSelection FromFootprint(Footprint footprint) => new FootprintSelection(footprint);

        public abstract PositionedFootprint GetPositionedFootprint(Level level, Position2 position);

        private class FootprintSelection : TileSelection
        {
            private readonly Footprint footprint;

            public FootprintSelection(Footprint footprint)
            {
                this.footprint = footprint;
            }

            public override PositionedFootprint GetPositionedFootprint(Level level, Position2 position)
            {
                return new PositionedFootprint(level, footprint, footprint.RootTileClosestToWorldPosition(level, position));
            }
        }

        private class TriangleSelection : TileSelection
        {
            public override PositionedFootprint GetPositionedFootprint(Level level, Position2 position)
            {
                var currentTile = level.GetTile(position);
                var tileCenter = level.GetPosition(currentTile);
                var direction2 = (position - tileCenter).Direction;
                var directionEnum = (direction2 - 30.Degrees()).Hexagonal();

                switch (directionEnum)
                {
                    case Direction.Left:
                        return new PositionedFootprint(level, Footprint.TriangleDown, currentTile.Neighbour(Direction.Left));
                    case Direction.DownLeft:
                        return new PositionedFootprint(level, Footprint.TriangleUp, currentTile.Neighbour(Direction.DownLeft));
                    case Direction.DownRight:
                        return new PositionedFootprint(level, Footprint.TriangleDown, currentTile);
                    case Direction.Right:
                        return new PositionedFootprint(level, Footprint.TriangleUp, currentTile);
                    case Direction.UpRight:
                        return new PositionedFootprint(level, Footprint.TriangleDown, currentTile.Neighbour(Direction.UpLeft));
                    case Direction.UpLeft:
                        return new PositionedFootprint(level, Footprint.TriangleUp, currentTile.Neighbour(Direction.Left));
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
