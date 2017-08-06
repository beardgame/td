using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Bearded.TD.Game.Tiles;
using Bearded.Utilities.SpaceTime;
using OpenTK;
using static Bearded.TD.Constants.Game.World;
using Extensions = Bearded.Utilities.Linq.Extensions;

namespace Bearded.TD.Game.World
{
    struct LevelRayCaster<TTileInfo>
    {
        private static readonly Vector2[] directionVectorsWithHexWidthLength =
            Extensions.Prepend(Tiles.Extensions.Directions.Select(d => d.Vector() * HexagonWidth), Vector2.Zero).ToArray();
        
        private Direction centerDirection, leftDirection, rightDirection;
        private float nextCenterIntersection, centerFullStep, centerHalfStep;
        private float nextLeftIntersection, leftFullStep, leftHalfStep;
        private float nextRightIntersection, rightFullStep, rightHalfStep;
        private Tile<TTileInfo> tile;

        private bool reachedFinalTile => nextCenterIntersection > 1 && nextLeftIntersection > 1 && nextRightIntersection > 1;

        public IEnumerable<Tile<TTileInfo>> EnumerateTiles(Level<TTileInfo> level, Ray ray, Tile<TTileInfo> startTile)
        {
#if DEBUG
            if (level.GetTile(ray.Start) != startTile)
                throw new ArgumentException("Ray must start on given start tile.");
#endif
            initialise(level, ray, startTile);

            yield return tile;

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
        
        public TiledRayHitResult<TTileInfo> ShootRay(Level<TTileInfo> level, Ray ray, Tile<TTileInfo> startTile)
        {
#if DEBUG
            if (level.GetTile(ray.Start) != startTile)
                throw new ArgumentException("Ray must start on given start tile.");
#endif
            initialise(level, ray, startTile);

            if (reachedFinalTile)
            {
                return RayHitResult.Miss(ray).OnTile(startTile, new Difference2());
            }

            while (true)
            {
                goToNextTile();

                if (reachedFinalTile)
                {
                    break;
                }
            }

            return RayHitResult.Miss(ray).OnTile(tile, new Difference2());
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
            if (nextCenterIntersection < nextLeftIntersection)
            {
                if (nextCenterIntersection < nextRightIntersection)
                {
                    stepCenter();
                }
                else
                {
                    stepRight();
                }
            }
            else if (nextLeftIntersection < nextRightIntersection)
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
            nextCenterIntersection += centerFullStep;
            nextLeftIntersection += leftHalfStep;
            nextRightIntersection += rightHalfStep;
            tile = tile.Neighbour(centerDirection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void stepLeft()
        {
            nextCenterIntersection += centerHalfStep;
            nextLeftIntersection += leftFullStep;
            nextRightIntersection -= rightHalfStep;
            tile = tile.Neighbour(leftDirection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void stepRight()
        {
            nextCenterIntersection += centerHalfStep;
            nextLeftIntersection -= leftHalfStep;
            nextRightIntersection += rightFullStep;
            tile = tile.Neighbour(rightDirection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (float Start, float FullStep, float HalfStep) getRayTraceParameters(Vector2 rayStart, Vector2 rayDir, Direction side)
        {
            var sideVector = directionVectorsWithHexWidthLength[(int)side];

            var rayDirProjection = Vector2.Dot(rayDir, sideVector);
            var stepLength = 1 / rayDirProjection;

            var pointOnSide = side.CornerAfter() * HexagonSide;
            var startOffset = rayStart - pointOnSide;
            var startOffsetProjection = Vector2.Dot(startOffset, sideVector);
            var startStepOffset = -startOffsetProjection * stepLength;

            return (startStepOffset, stepLength, stepLength * 0.5f);
        }
    }
}