﻿using System;
using amulware.Graphics;
using Bearded.TD.Game.World;
using Bearded.Utilities;
using OpenTK;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Rendering.Deferred
{
    class LevelGeometry
    {
        private static readonly Color openColor = Color.White;

        private readonly IndexedSurface<LevelVertex> surface;

        public LevelGeometry(IndexedSurface<LevelVertex> surface)
        {
            this.surface = surface;
        }

        public void DrawTile(Vector2 position,
            DrawableTileGeometry tile, DrawableTileGeometry rightTile,
            DrawableTileGeometry upRightTile, DrawableTileGeometry downRightTile)
        {
            var (selfZ, selfScale) = getHeightAndScale(tile);
            var (rightZ, rightScale) = getHeightAndScale(rightTile);
            var (downZ, downScale) = getHeightAndScale(downRightTile);
            var (upZ, upScale) = getHeightAndScale(upRightTile);

            var selfCorners = cornerOffsetsWithScale(selfScale);

            var topV = (position + new Vector2(0, selfCorners.topY)).WithZ(selfZ);
            var bottomV = (position + new Vector2(0, -selfCorners.topY)).WithZ(selfZ);
            var topLeftV = (position + new Vector2(-selfCorners.sideX, selfCorners.sideY)).WithZ(selfZ);
            var bottomLeftV = (position + new Vector2(-selfCorners.sideX, -selfCorners.sideY)).WithZ(selfZ);
            var topRightV = (position + new Vector2(selfCorners.sideX, selfCorners.sideY)).WithZ(selfZ);
            var bottomRightV = (position + new Vector2(selfCorners.sideX, -selfCorners.sideY)).WithZ(selfZ);

            var n = Vector3.UnitZ;
            var c = openColor;
            
            addHex(topV, topRightV, bottomRightV, bottomV, bottomLeftV, topLeftV, n, c);

            var rightCorners = cornerOffsetsWithScale(rightScale);
            var downCorners = cornerOffsetsWithScale(downScale);
            var upCorners = cornerOffsetsWithScale(upScale);

            var rightPosition = position + new Vector2(HexagonDistanceX, 0);
            var upPosition = position + new Vector2(HexagonDistanceX / 2, HexagonDistanceY);
            var downPosition = position + new Vector2(HexagonDistanceX / 2, -HexagonDistanceY);

            var upBottomLeftV = (upPosition + new Vector2(-upCorners.sideX, -upCorners.sideY)).WithZ(upZ);
            var upBottomV = (upPosition + new Vector2(0, -upCorners.topY)).WithZ(upZ);

            if (upRightTile.HasKnownType)
            {
                addQuad(topV, upBottomLeftV, upBottomV, topRightV, c);
            }

            var downTopV = (downPosition + new Vector2(0, downCorners.topY)).WithZ(downZ);
            var downTopLeftV = (downPosition + new Vector2(-downCorners.sideX, downCorners.sideY)).WithZ(downZ);

            if (downRightTile.HasKnownType)
            {
                addQuad(bottomV, bottomRightV, downTopV, downTopLeftV, c);
            }

            var rightTopLeftV = (rightPosition + new Vector2(-rightCorners.sideX, rightCorners.sideY)).WithZ(rightZ);
            var rightBottomLeftV = (rightPosition + new Vector2(-rightCorners.sideX, -rightCorners.sideY)).WithZ(rightZ);

            if (rightTile.HasKnownType)
            {
                addQuad(topRightV, rightTopLeftV, rightBottomLeftV, bottomRightV, c);
            }

            if (rightTile.HasKnownType && upRightTile.HasKnownType)
            {
                addTriangle(topRightV, upBottomV, rightTopLeftV, c);
            }
            
            if (rightTile.HasKnownType && downRightTile.HasKnownType)
            {
                addTriangle(bottomRightV, rightBottomLeftV, downTopV, c);
            }
        }

        private (float height, float scale) getHeightAndScale(DrawableTileGeometry geometry)
        {
            var drawInfo = geometry.DrawInfo;
            return (drawInfo.Height.NumericValue, drawInfo.HexScale);
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
                vertex(v0, n, Vector2.Zero, c),
                vertex(v1, n, Vector2.Zero, c),
                vertex(v2, n, Vector2.Zero, c)
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
                vertex(v0, n, Vector2.Zero, c),
                vertex(v1, n, Vector2.Zero, c),
                vertex(v2, n, Vector2.Zero, c),
                vertex(v3, n, Vector2.Zero, c)
            );
        }

        private void addHex(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 n, Color c)
        {
            var vertices = surface.WriteVerticesDirectly(6, out var vOffset);

            vertices[vOffset] = vertex(v0, n, Vector2.Zero, c);
            vertices[vOffset + 1] = vertex(v1, n, Vector2.Zero, c);
            vertices[vOffset + 2] = vertex(v2, n, Vector2.Zero, c);
            vertices[vOffset + 3] = vertex(v3, n, Vector2.Zero, c);
            vertices[vOffset + 4] = vertex(v4, n, Vector2.Zero, c);
            vertices[vOffset + 5] = vertex(v5, n, Vector2.Zero, c);

            var indices = surface.WriteIndicesDirectly(12, out var iOffset);

            indices[iOffset++] = vOffset;
            indices[iOffset++] = (ushort) (vOffset + 1);
            indices[iOffset++] = (ushort) (vOffset + 2);

            indices[iOffset++] = vOffset;
            indices[iOffset++] = (ushort) (vOffset + 2);
            indices[iOffset++] = (ushort) (vOffset + 3);

            indices[iOffset++] = vOffset;
            indices[iOffset++] = (ushort) (vOffset + 3);
            indices[iOffset++] = (ushort) (vOffset + 4);

            indices[iOffset++] = vOffset;
            indices[iOffset++] = (ushort) (vOffset + 4);
            indices[iOffset] = (ushort) (vOffset + 5);
        }

        private static LevelVertex vertex(Vector3 v, Vector3 n, Vector2 uv, Color c)
        {
            var a = (1 - Math.Abs(v.Z * v.Z * 1f)).Clamped(0f, 1);

            return new LevelVertex(v, n, uv, new Color(c * a, c.A));
        }
    }
}
