using System;
using System.Linq;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Navigation;

sealed partial class MultipleSinkNavigationSystem
{
    public void DrawDebug(CoreDrawers drawers, bool drawWeights)
    {
        var shapeDrawer = drawers.ConsoleBackground;
        var textDrawer = drawers.InGameFont;

        const float lineWidth = Constants.Game.World.HexagonSide * 0.05f;
        const float fontHeight = Constants.Game.World.HexagonSide;

        var weightsFontColor = Color.Orange;

        const float w = Constants.Game.World.HexagonDistanceX * 0.5f - 0.1f;
        const float h = Constants.Game.World.HexagonDistanceY * 0.5f - 0.1f;

        if (drawWeights)
        {
            var i = 0;
            foreach (var tile in updateFront)
            {
                var p = Level.GetPosition(tile).NumericValue;

                shapeDrawer.FillRectangle(p.X - w, p.Y - h, w * 2, h * 2, weightsFontColor * 0.3f);

                textDrawer.DrawLine(
                    xyz: p.WithZ(),
                    text: $"{i}",
                    fontHeight: fontHeight,
                    alignHorizontal: 0.5f,
                    alignVertical: 1f,
                    parameters: weightsFontColor);
                i++;
            }
        }

        foreach (var tile in graph)
        {
            var node = graph[tile];

            var p = Level.GetPosition(tile).NumericValue;

            var d = node.Direction.Vector() * Constants.Game.World.HexagonWidth;

            if (!node.IsSink)
            {
                var pointsToTile = tile.Neighbor(node.Direction);
                var pointsToNode = graph[pointsToTile];

                if (!pointsToNode.IsInvalid && pointsToNode.Distance >= node.Distance)
                {
                    shapeDrawer.FillRectangle(p.X - w, p.Y - h, w * 2, h * 2, Color.Red * 0.3f);
                }

                var color = (pointsToNode.IsInvalid ? Color.Red : Color.DarkGreen) * 0.8f;
                shapeDrawer.DrawLine(p, p + d, lineWidth, color);
                shapeDrawer.DrawLine(p + d, p + .9f * d + .1f * d.PerpendicularRight, lineWidth, color);
                shapeDrawer.DrawLine(p + d, p + .9f * d + .1f * d.PerpendicularLeft, lineWidth, color);
            }

            if (drawWeights && !node.IsInvalid)
            {
                var color = Color.Yellow;
                var distance = node.Distance;
                if (distance >= backupSinkDistance)
                {
                    distance -= backupSinkDistance;
                    color = Color.Red;
                }

                if (!node.IsSink && level.ValidNeighboursOf(tile).Select(t => graph[t])
                        .Any(n => !n.IsInvalid && !n.IsSink && Math.Abs(n.Distance - node.Distance) > 1))
                {
                    color = Color.MediumPurple;
                }

                textDrawer.DrawLine(
                    xyz: p.WithZ(),
                    text: $"{distance}",
                    fontHeight: fontHeight,
                    alignHorizontal: 0.5f,
                    parameters: color);
            }
        }
    }
}
