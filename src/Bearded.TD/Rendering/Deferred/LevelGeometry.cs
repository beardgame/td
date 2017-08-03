using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.Utilities.Math;
using OpenTK;
using static Bearded.TD.Constants.Game.World;
using static Bearded.TD.Game.Tiles.Direction;

namespace Bearded.TD.Rendering.Deferred
{
    class LevelGeometry
    {
        private static readonly Color openColor = Color.White;
        private static readonly Color closedColor = Color.White;

        private readonly IndexedSurface<LevelVertex> surface;

        public LevelGeometry(IndexedSurface<LevelVertex> surface)
        {
            this.surface = surface;
        }

        public void DrawTile(Vector2 position, bool isOpen, Directions openDirections)
        {
            var rightIsOpen = openDirections.Includes(Right);
            var downIsOpen = openDirections.Includes(DownRight);
            var upIsOpen = openDirections.Includes(UpRight);

            var floorZ = 0f;
            var wallZ = 0.5f;
            var selfZ = isOpen ? floorZ : wallZ;
            var rightZ = rightIsOpen ? floorZ : wallZ;
            var downZ = downIsOpen ? floorZ : wallZ;
            var upZ = upIsOpen ? floorZ : wallZ;

            var selfScale = 0.8f;
            var rightScale = 0.8f;
            var downScale = 0.8f;
            var upScale = 0.8f;
            
            var selfCorners = cornerOffsetsWithScale(selfScale);

            var topV = (position + new Vector2(0, selfCorners.topY)).WithZ(selfZ);
            var bottomV = (position + new Vector2(0, -selfCorners.topY)).WithZ(selfZ);
            var topLeftV = (position + new Vector2(-selfCorners.sideX, selfCorners.sideY)).WithZ(selfZ);
            var bottomLeftV = (position + new Vector2(-selfCorners.sideX, -selfCorners.sideY)).WithZ(selfZ);
            var topRightV = (position + new Vector2(selfCorners.sideX, selfCorners.sideY)).WithZ(selfZ);
            var bottomRightV = (position + new Vector2(selfCorners.sideX, -selfCorners.sideY)).WithZ(selfZ);

            var n = Vector3.UnitZ;
            var c = openColor;

            addTriangle(topV, topRightV, bottomRightV, n, c);
            addTriangle(topV, bottomRightV, bottomV, n, c);
            addTriangle(topV, bottomV, bottomLeftV, n, c);
            addTriangle(topV, bottomLeftV, topLeftV, n, c);

            var rightCorners = cornerOffsetsWithScale(rightScale);
            var downCorners = cornerOffsetsWithScale(downScale);
            var upCorners = cornerOffsetsWithScale(upScale);

            var rightPosition = position + new Vector2(HexagonDistanceX, 0);
            var upPosition = position + new Vector2(HexagonDistanceX / 2, HexagonDistanceY);
            var downPosition = position + new Vector2(HexagonDistanceX / 2, -HexagonDistanceY);

            var upBottomLeftV = (upPosition + new Vector2(-upCorners.sideX, -upCorners.sideY)).WithZ(upZ);
            var upBottomV = (upPosition + new Vector2(0, -upCorners.topY)).WithZ(upZ);
            
            addQuad(topV, upBottomLeftV, upBottomV, topRightV, c);

            var downTopV = (downPosition + new Vector2(0, downCorners.topY)).WithZ(downZ);
            var downTopLeftV = (downPosition + new Vector2(-selfCorners.sideX, selfCorners.sideY)).WithZ(downZ);

            addQuad(bottomV, bottomRightV, downTopV, downTopLeftV, c);

            var rightTopLeftV = (rightPosition + new Vector2(-rightCorners.sideX, rightCorners.sideY)).WithZ(rightZ);
            var rightBottomLeftV = (rightPosition + new Vector2(-rightCorners.sideX, -rightCorners.sideY)).WithZ(rightZ);

           addQuad(topRightV, rightTopLeftV, rightBottomLeftV, bottomRightV, c);

            addTriangle(topRightV, upBottomV, rightTopLeftV, c);
            addTriangle(bottomRightV, rightBottomLeftV, downTopV, c);
        }

        private static (float topY, float sideY, float sideX) cornerOffsetsWithScale(float scale)
            => (HexagonSide * scale, HexagonSide / 2 * scale, HexagonWidth / 2 * scale);

        private void addTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Color c)
        {
            var normal = -Vector3.Cross(v1 - v0, v2 - v0).Normalized();
            addTriangle(v0, v1, v2, normal, c);
        }


        private void addTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 n, Color c)
        {
            surface.AddTriangle(
                new LevelVertex(v0, n, Vector2.Zero, c),
                new LevelVertex(v1, n, Vector2.Zero, c),
                new LevelVertex(v2, n, Vector2.Zero, c)
            );
        }

        private void addQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Color c)
        {
            var normal = -Vector3.Cross(v1 - v0, v3 - v0).Normalized();
            addQuad(v0, v1, v2, v3, normal, c);
        }

        private void addQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 n, Color c)
        {
            surface.AddQuad(
                new LevelVertex(v0, n, Vector2.Zero, c),
                new LevelVertex(v1, n, Vector2.Zero, c),
                new LevelVertex(v2, n, Vector2.Zero, c),
                new LevelVertex(v3, n, Vector2.Zero, c)
            );
        }
    }
}
