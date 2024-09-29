using System;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Rendering.Deferred.Level;

static class HeightmapToLevelRendererHelpers
{
    public readonly struct Grid
    {
        public float Scale { get; private init; }
        public float CellColumnsHalf { get; private init; }
        public float CellRowsHalf { get; private init; }
        public Vector2 TilingX { get; private init; }
        public Vector2 TilingY { get; private init; }

        public static Grid For(
            Tiles.Level level, float scaleSetting, Vector2 tilingX, Vector2 tilingY)
        {
            var tileDiameter = level.Radius * 2 + 1;

            var widthToCover = tileDiameter * 0.5f * HexagonDistanceX;
            var heightToCover = tileDiameter * 0.5f * HexagonDistanceY;

            var gridMeshWidth = tilingX.X;
            var gridMeshHeight = tilingY.Y;

            const float baseScale = 1f;
            var idealScale = baseScale / scaleSetting;

            var cellWidth = idealScale * gridMeshWidth;
            var cellHeight = idealScale * gridMeshHeight;

            var cellColumnsHalf = MoreMath.CeilToInt(widthToCover / cellWidth);
            var cellRowsHalf = MoreMath.CeilToInt(heightToCover / cellHeight);

            var scale = (tileDiameter * 0.5f * HexagonDistanceX) / cellColumnsHalf / gridMeshWidth;

            return new Grid
            {
                CellColumnsHalf = cellColumnsHalf,
                CellRowsHalf = cellRowsHalf,
                Scale = scale,
                TilingX = tilingX * scale,
                TilingY = tilingY * scale,
            };
        }

        public void IterateCellsIn(Rectangle bounds, Action<Vector2> action)
        {
            var cellVisibleWidth = TilingX.X + TilingY.X;

            for (var y = -CellRowsHalf; y < CellRowsHalf; y++)
            {
                var rowMin = y * TilingY.Y;
                var rowMax = rowMin + TilingY.Y;

                if (rowMin > bounds.Bottom || rowMax < bounds.Top)
                    continue;

                var xMin = Math.Max(-CellColumnsHalf, -CellColumnsHalf - y - 1);
                var xMax = Math.Min(CellColumnsHalf, CellColumnsHalf - y);

                for (var x = xMin; x < xMax; x++)
                {
                    var offset = x * TilingX + y * TilingY;

                    if (offset.X > bounds.Right || offset.X + cellVisibleWidth < bounds.Left)
                        continue;

                    action(offset);
                }
            }
        }
        public void IterateAllCells(Action<Vector2> action)
        {
            for (var y = -CellRowsHalf; y < CellRowsHalf; y++)
            {
                var xMin = Math.Max(-CellColumnsHalf, -CellColumnsHalf - y - 1);
                var xMax = Math.Min(CellColumnsHalf, CellColumnsHalf - y);

                for (var x = xMin; x < xMax; x++)
                {
                    var offset = x * TilingX + y * TilingY;
                    action(offset);
                }
            }
        }
    }

    public static void IterateLevelCells(
        Rectangle bounds,
        Grid grid,
        Action<Vector2> action)
    {
        var (cellColumnsHalf, cellRowsHalf) = (grid.CellColumnsHalf, grid.CellRowsHalf);
        var (tilingX, tilingY) = (grid.TilingX, grid.TilingY);

        var cellVisibleWidth = tilingX.X + tilingY.X;

        for (var y = -cellRowsHalf; y < cellRowsHalf; y++)
        {
            var rowMin = y * tilingY.Y;
            var rowMax = rowMin + tilingY.Y;

            if (rowMin > bounds.Bottom || rowMax < bounds.Top)
                continue;

            var xMin = Math.Max(-cellColumnsHalf, -cellColumnsHalf - y - 1);
            var xMax = Math.Min(cellColumnsHalf, cellColumnsHalf - y);

            for (var x = xMin; x < xMax; x++)
            {
                var offset = x * tilingX + y * tilingY;

                if (offset.X > bounds.Right || offset.X + cellVisibleWidth < bounds.Left)
                    continue;

                action(offset);
            }
        }
    }

}
