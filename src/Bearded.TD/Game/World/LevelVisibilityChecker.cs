using System;
using System.Collections.Generic;
using Bearded.TD.Tiles;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.World;
using static Bearded.TD.Game.World.VisibilityFlags;

namespace Bearded.TD.Game.World
{
    [Flags]
    enum VisibilityFlags : byte
    {
        FullyVisible = 0,
        Blocking = 1,
        BlockedRight = 2,
        BlockedLeft = 4,
        BlockedLeftAndRight = BlockedLeft | BlockedRight,
        BlockedFully = BlockedLeftAndRight | 8,
    }

    struct TileVisibility
    {
        public VisibilityFlags Flags { get; }
        public float VisiblePercentage { get; }

        public TileVisibility(VisibilityFlags flags, float visiblePercentage)
        {
            Flags = flags;
            VisiblePercentage = visiblePercentage;
        }

        public bool IsBlocking => Flags.HasFlag(Blocking);
        public bool IsFullyVisible => Flags == FullyVisible;
        public bool RightIsBlocked => Flags.HasFlag(BlockedRight);
        public bool LeftIsBlocked => Flags.HasFlag(BlockedLeft);
        public bool IsFullyBlocked => Flags.HasFlag(BlockedFully);

        public static implicit operator TileVisibility ((VisibilityFlags flags, float visiblePercentage) parameters)
            => new TileVisibility(parameters.flags, parameters.visiblePercentage);
    }

    struct LevelVisibilityChecker
    {
        private IEnumerable<Tile> tiles;
        private Tile startTile;
        private int tileRadius;
        private List<(Direction2 start, Angle width)> intervalsBlocked;
        private int currentIntervalIndex;
        private Position2 origin;
        private Level level;

        public IEnumerable<(Tile tile, TileVisibility visibility)> EnumerateVisibleTiles(
            Level level, Position2 origin, Unit radius, Func<Tile, bool> blocksVisibility)
        {
            initialise(level, origin, radius);

            if (!startTile.IsValid || blocksVisibility(startTile))
                yield break;

            intervalsBlocked = new List<(Direction2, Angle)>(10);
            
            tiles = level.Tilemap.SpiralCenteredAt(startTile, tileRadius);

            foreach (var tile in tiles)
            {
                var isBlocking = blocksVisibility(tile);

                if (intervalsBlocked.Count == 0)
                {
                    if (!isBlocking)
                    {
                        yield return (tile, (FullyVisible, 1));
                    }
                    else
                    {
                        var interval = intervalForTile(tile);
                        intervalsBlocked.Add(interval);
                        currentIntervalIndex = 0;
                        yield return (tile, (Blocking, 1));
                    }
                }
                else
                {
                    var (start, width) = intervalForTile(tile);

                    var currentInterval = intervalsBlocked[currentIntervalIndex];
                    
                    for (var i = 0; i < intervalsBlocked.Count; i++)
                    {
                        var nextIndex = (currentIntervalIndex + 1) % intervalsBlocked.Count;
                        var nextInterval = intervalsBlocked[nextIndex];

                        var gapWidth = Angle.BetweenPositive(currentInterval.start + currentInterval.width, nextInterval.start);

                        var currentStartOffset = Angle.BetweenPositive(currentInterval.start, start);

                        var startIsInsideCurrent = currentStartOffset <= currentInterval.width;

                        if (startIsInsideCurrent)
                        {
                            var endIsInsideCurrent = currentStartOffset + width <= currentInterval.width;

                            if (endIsInsideCurrent)
                            {
                                // ----| |--
                                //  |-|
                                yield return (tile, (BlockedFully, 0));
                                break;
                            }

                            var endIsInsideGap = currentStartOffset + width < currentInterval.width + gapWidth;

                            if (endIsInsideGap)
                            {
                                // --| |--
                                //  |-|
                                var visiblePercentage = (width + currentStartOffset - currentInterval.width).Radians / width.Radians;
                                if (isBlocking)
                                {
                                    var newIntervalWidth = currentStartOffset + width;
                                    intervalsBlocked[currentIntervalIndex] = (currentInterval.start, newIntervalWidth);
                                    yield return (tile, (BlockedRight | Blocking, visiblePercentage));
                                }
                                else
                                {
                                    yield return (tile, (BlockedRight, visiblePercentage));
                                }
                                break;
                            }

                            // end is inside next
                            {
                                // --| |--
                                //  |---|
                                var visiblePercentage = gapWidth.Radians / width.Radians;

                                if (isBlocking)
                                {
                                    if (intervalsBlocked.Count == 1)
                                    {
                                        // tile covers single remaining gap
                                        yield return (tile, (BlockedLeftAndRight | Blocking, visiblePercentage));
                                        yield break;
                                    }

                                    // merge intervals
                                    var newIntervalWidth =currentInterval.width + gapWidth + nextInterval.width;

                                    intervalsBlocked[currentIntervalIndex] = (currentInterval.start, newIntervalWidth);
                                    intervalsBlocked.RemoveAt(nextIndex);
                                    if (currentIntervalIndex > nextIndex)
                                        currentIntervalIndex--;

                                    yield return (tile, (BlockedLeftAndRight | Blocking, visiblePercentage));
                                }
                                else
                                {
                                    yield return (tile, (BlockedLeftAndRight, visiblePercentage));
                                }
                                break;
                            }
                        }

                        var startIsInsideGap = currentStartOffset <= currentInterval.width + gapWidth;

                        if (startIsInsideGap)
                        {
                            var endIsInsideGap = currentStartOffset + width < currentInterval.width + gapWidth;

                            if (endIsInsideGap)
                            {
                                // -|   |--
                                //   |-|
                                if (isBlocking)
                                {
                                    intervalsBlocked.Insert(currentIntervalIndex + 1, (start, width));
                                    currentIntervalIndex++;

                                    yield return (tile, (FullyVisible | Blocking, 1));
                                }
                                else
                                {
                                    yield return (tile, (FullyVisible, 1));
                                }
                                break;
                            }

                            // end is inside next

                            // -| |--
                            //   |-|
                            var visibilityPercentage = (gapWidth + currentInterval.width - currentStartOffset).Radians / width.Radians;

                            if (isBlocking)
                            {
                                var newIntervalWidth = nextInterval.width + gapWidth + currentInterval.width - currentStartOffset;

                                intervalsBlocked[nextIndex] = (start, newIntervalWidth);
                                currentIntervalIndex = nextIndex;

                                yield return (tile, (BlockedLeft | Blocking, visibilityPercentage));
                            }
                            else
                            {
                                yield return (tile, (BlockedLeft, visibilityPercentage));
                            }
                            break;
                        }

                        currentInterval = nextInterval;
                        currentIntervalIndex = nextIndex;
                    }
                }

            }
        }

        private (Direction2 start, Angle width) intervalForTile(Tile tile)
        {
            var center = level.GetPosition(tile);

            var centerAngle = Direction2.Between(origin.NumericValue, center.NumericValue);

            var distance = (center - origin).Length;

            // https://en.wikipedia.org/wiki/Angular_diameter#Formula
            // correct formula is 2*asin(hexD/(2*distance))) for circular tiles
            // the approximiation 2*atan(hexD/(2*distance))) flattens along the difference vector
            // the effect is that even when being very close there is no risk of rounding causing NaN/infinity issues

            const float tileCircleRadius = (HexagonDiameter + HexagonWidth) * 0.5f;

            var angularRadius = (2 * (float)Math.Atan(tileCircleRadius * 0.25 / distance.NumericValue)).Radians();

            return (centerAngle - angularRadius, angularRadius * 2);
        }

        private void initialise(Level level, Position2 origin, Unit radius)
        {
            this.level = level;
            this.origin = origin;
            tileRadius = (int)(radius.NumericValue * (1 / HexagonWidth) + HexagonWidth);
            startTile = level.GetTile(origin);
        }
    }
}
