using System.Linq;
using System.Runtime.CompilerServices;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Geometry;
using OpenToolkit.Mathematics;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Game.Simulation.World
{
    struct LevelRayCaster
    {
        private static readonly Vector2[] directionVectorsWithHexWidthLength =
            Extensions.Directions
                .Select(d => d.Vector() * HexagonWidth)
                .Prepend(Vector2.Zero).ToArray();

        private Direction centerDirection, leftDirection, rightDirection;
        private float nextCenterIntersection, centerFullStep, centerHalfStep;
        private float nextLeftIntersection, leftFullStep, leftHalfStep;
        private float nextRightIntersection, rightFullStep, rightHalfStep;
        private float currentRayFactor;
        private Tile tile;
        private bool firstMove;

        private bool reachedFinalTile => nextCenterIntersection > 1 && nextLeftIntersection > 1 && nextRightIntersection > 1;

        public Tile Current => tile;
        public float CurrentRayFactor => currentRayFactor;

        public void StartEnumeratingTiles(Level level, Ray ray)
        {
            initialise(ray, Level.GetTile(ray.Start));
            firstMove = true;
        }

        public bool MoveNext()
        {
            if (firstMove)
            {
                firstMove = false;
                return true;
            }

            if (reachedFinalTile)
                return false;

            goToNextTile();

            return true;
        }

        public LevelRayCaster GetEnumerator() => this;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext(out Tile current)
        {
            var value = MoveNext();
            current = tile;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void initialise(Ray ray, Tile startTile)
        {
            var startTilePosition = Level.GetPosition(startTile);
            var relativeStartTilePosition = (ray.Start - startTilePosition).NumericValue;
            var rayDirection = ray.Direction.NumericValue;

            centerDirection = ray.Direction.Direction.Hexagonal();
            leftDirection = centerDirection.Next();
            rightDirection = centerDirection.Previous();

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
