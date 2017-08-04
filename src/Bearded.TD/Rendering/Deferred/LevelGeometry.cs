using System;
using amulware.Graphics;
using Bearded.Utilities.Math;
using OpenTK;
using static Bearded.TD.Constants.Game.World;
using TileType = Bearded.TD.Game.World.TileInfo.Type;

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
            TileType tileType, TileType rightTileType,
            TileType upRightTileType, TileType downRightTileType)
        {
            var (selfZ, selfScale) = getHeightAndScale(tileType);
            var (rightZ, rightScale) = getHeightAndScale(rightTileType);
            var (downZ, downScale) = getHeightAndScale(downRightTileType);
            var (upZ, upScale) = getHeightAndScale(upRightTileType);
            
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

            if (upRightTileType != TileType.Unknown)
            {
                addQuad(topV, upBottomLeftV, upBottomV, topRightV, c);
            }

            var downTopV = (downPosition + new Vector2(0, downCorners.topY)).WithZ(downZ);
            var downTopLeftV = (downPosition + new Vector2(-downCorners.sideX, downCorners.sideY)).WithZ(downZ);

            if (downRightTileType != TileType.Unknown)
            {
                addQuad(bottomV, bottomRightV, downTopV, downTopLeftV, c);
            }

            var rightTopLeftV = (rightPosition + new Vector2(-rightCorners.sideX, rightCorners.sideY)).WithZ(rightZ);
            var rightBottomLeftV = (rightPosition + new Vector2(-rightCorners.sideX, -rightCorners.sideY)).WithZ(rightZ);

            if (rightTileType != TileType.Unknown)
            {
                addQuad(topRightV, rightTopLeftV, rightBottomLeftV, bottomRightV, c);
            }

            if (rightTileType != TileType.Unknown && upRightTileType != TileType.Unknown)
            {
                addTriangle(topRightV, upBottomV, rightTopLeftV, c);
            }
            
            if (rightTileType != TileType.Unknown && downRightTileType != TileType.Unknown)
            {
                addTriangle(bottomRightV, rightBottomLeftV, downTopV, c);
            }
        }

        private static (float, float) getHeightAndScale(TileType tileType)
        {
            switch (tileType)
            {
                case TileType.Unknown:
                    return (0, 0);
                case TileType.Floor:
                    return (0, 0.8f);
                case TileType.Wall:
                    return (0.5f, 0.5f);
                case TileType.Crevice:
                    return (-3, 0.5f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(tileType), tileType, null);
            }
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

        private void addHex(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 n, Color c)
        {
            var vertices = surface.WriteVerticesDirectly(6, out var vOffset);

            vertices[vOffset] = new LevelVertex(v0, n, Vector2.Zero, c);
            vertices[vOffset + 1] = new LevelVertex(v1, n, Vector2.Zero, c);
            vertices[vOffset + 2] = new LevelVertex(v2, n, Vector2.Zero, c);
            vertices[vOffset + 3] = new LevelVertex(v3, n, Vector2.Zero, c);
            vertices[vOffset + 4] = new LevelVertex(v4, n, Vector2.Zero, c);
            vertices[vOffset + 5] = new LevelVertex(v5, n, Vector2.Zero, c);

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
    }
}
