using System;
using amulware.Graphics;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using OpenTK;

namespace Bearded.TD.Rendering.InGameUI
{
    static class TileAreaBorderRenderer
    {
        public static void Render(
            TileAreaBorder border, PrimitiveGeometry geometry,
            Color color, float lineWidth = 0.1f)
        {
            geometry.Color = color;
            geometry.LineWidth = lineWidth;
            
            border.Visit(t =>
            {
                var (tile, direction) = t;

                var center = Level.GetPosition(tile).NumericValue;
                var offset1 = direction.CornerBefore() * Constants.Game.World.HexagonSide;
                var offset2 = direction.CornerAfter() * Constants.Game.World.HexagonSide;
                
                geometry.DrawLine(center + offset1, center + offset2);
            });
        }
        
        public static void Render(
            TileAreaBorder border, PrimitiveGeometry geometry, Func<Position2, Position2, Color?> getLineColor, float lineWidth = 0.1f)
        {
            geometry.LineWidth = lineWidth;
            
            border.Visit(t =>
            {
                var (tile, direction) = t;

                var center = Level.GetPosition(tile);
                var point1 = center + new Difference2(direction.CornerBefore() * Constants.Game.World.HexagonSide);
                var point2 = center + new Difference2(direction.CornerAfter() * Constants.Game.World.HexagonSide);

                var color = getLineColor(point1, point2);

                if (color == null)
                    return;

                geometry.Color = color.Value;
                
                geometry.DrawLine(point1.NumericValue, point2.NumericValue);
            });
        }
    }
}
