using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using OpenTK;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Game.World
{
    struct LevelRayCaster<TTileInfo>
    {
        private static readonly Vector2[] directionVectorsWithHexWidthLength =
            Tiles.Extensions.Directions
                .Select(d => d.Vector() * HexagonWidth)
                .Prepend(Vector2.Zero).ToArray();
        
        private Direction centerDirection, leftDirection, rightDirection;
        private float nextCenterIntersection, centerFullStep, centerHalfStep;
        private float nextLeftIntersection, leftFullStep, leftHalfStep;
        private float nextRightIntersection, rightFullStep, rightHalfStep;
        private float currentRayFactor;
        private Tile<TTileInfo> tile;

        private bool reachedFinalTile => nextCenterIntersection > 1 && nextLeftIntersection > 1 && nextRightIntersection > 1;
        
        public IEnumerable<Tile<TTileInfo>> EnumerateTiles(Level<TTileInfo> level, Ray ray)
        {
            var startTile = level.GetTile(ray.Start);

            yield return startTile;

            initialise(level, ray, startTile);

            if (reachedFinalTile)
            {
                yield break;
            }

            while (true)
            {
                goToNextTile();

                yield return tile;

                if (reachedFinalTile)
                {
                    yield break;
                }
            }
        }

        public (Tile<TTileInfo>, Position2) GetEndOfRay(Level<TTileInfo> level, Ray ray, Tile<TTileInfo> startTile)
        {
            checkPreconditions(level, ray, startTile);

            initialise(level, ray, startTile);

            if (reachedFinalTile)
            {
                return (startTile, ray.Start + ray.Direction);
            }

            while (true)
            {
                goToNextTile();

                if (reachedFinalTile)
                {
                    break;
                }
            }

            return (tile, ray.Start + ray.Direction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void checkPreconditions(Level<TTileInfo> level, Ray ray, Tile<TTileInfo> startTile)
        {
#if DEBUG
            if (level.GetTile(ray.Start) != startTile)
                throw new ArgumentException("Ray must start on given start tile.");
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void initialise(Level<TTileInfo> level, Ray ray, Tile<TTileInfo> startTile)
        {
            var startTilePosition = level.GetPosition(startTile);
            var relativeStartTilePosition = (ray.Start - startTilePosition).NumericValue;
            var rayDirection = ray.Direction.NumericValue;

            centerDirection = ray.Direction.Direction.Hexagonal();
            leftDirection = centerDirection.NextLeft();
            rightDirection = centerDirection.NextRight();

            // project into single dimensional space aligned with the ray where the ray is of length 1
            (nextCenterIntersection, centerFullStep, centerHalfStep) = getRayTraceParameters(relativeStartTilePosition, rayDirection, centerDirection);
            (nextLeftIntersection, leftFullStep, leftHalfStep) = getRayTraceParameters(relativeStartTilePosition, rayDirection, leftDirection);
            (nextRightIntersection, rightFullStep, rightHalfStep) = getRayTraceParameters(relativeStartTilePosition, rayDirection, rightDirection);
            
            tile = startTile;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void goToNextTile()
        {
            // bias towards center for nicer edge case routes
            if (nextCenterIntersection <= nextLeftIntersection)
            {
                if (nextCenterIntersection <= nextRightIntersection)
                {
                    stepCenter();
                }
                else
                {
                    stepRight();
                }
            }
            else if (nextLeftIntersection <= nextRightIntersection)
            {
                stepLeft();
            }
            else
            {
                stepRight();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void stepCenter()
        {
            currentRayFactor = nextCenterIntersection;
            nextCenterIntersection += centerFullStep;
            nextLeftIntersection += leftHalfStep;
            nextRightIntersection += rightHalfStep;
            tile = tile.Neighbour(centerDirection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void stepLeft()
        {
            currentRayFactor = nextLeftIntersection;
            nextCenterIntersection += centerHalfStep;
            nextLeftIntersection += leftFullStep;
            nextRightIntersection -= rightHalfStep;
            tile = tile.Neighbour(leftDirection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void stepRight()
        {
            currentRayFactor = nextRightIntersection;
            nextCenterIntersection += centerHalfStep;
            nextLeftIntersection -= leftHalfStep;
            nextRightIntersection += rightFullStep;
            tile = tile.Neighbour(rightDirection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (float Start, float FullStep, float HalfStep) getRayTraceParameters(
            Vector2 rayStart, Vector2 rayDir, Direction side)
        {
            var sideVector = directionVectorsWithHexWidthLength[(int)side];

            var rayDirProjection = Vector2.Dot(rayDir, sideVector);
            
            // non-positive values may occur due to rounding issues
            // in case of (nearly) orthogonal vectors in the dot product above
            // returning halfStep 0 will ensure we avoid `infinity - infinity` NaN
            if (rayDirProjection <= 0)
                return (float.PositiveInfinity, float.PositiveInfinity, 0);

            var stepLength = 1 / rayDirProjection;

            var pointOnSide = side.CornerAfter() * HexagonSide;
            var startOffset = rayStart - pointOnSide;
            var startOffsetProjection = Vector2.Dot(startOffset, sideVector);
            var startStepOffset = -startOffsetProjection * stepLength;

            return (startStepOffset, stepLength, stepLength * 0.5f);
        }
    }
}