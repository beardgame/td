using System;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK;

namespace Bearded.TD.Rendering.InGameUI
{
    static class TileAreaBorderRenderer
    {
        public static void Render(GameState game, TileAreaBorder border,
            Color color, float lineWidth = 0.3f)
        {
            Render(game, border, _ => color, lineWidth);
        }
        
        public static void Render(GameState game, TileAreaBorder border,
            Func<Position2, Color?> getLineColor, float lineWidth = 0.3f)
        {
            var sprites = game.Meta.Blueprints.Sprites["particle"];
            var sprite = sprites.Sprites.GetSprite("halo");

            var offsetOuter = new Unit(Constants.Game.World.HexagonSide);
            var offsetInner = new Unit(Constants.Game.World.HexagonSide - lineWidth);
            
            border.Visit(t =>
            {
                var (tile, direction) = t;

                var center = Level.GetPosition(tile);
                var vector1 = direction.CornerBefore();
                var vector2 = direction.CornerAfter();
                var point1 = center + vector1 * offsetOuter;
                var point2 = center + vector2 * offsetOuter;

                var color1 = getLineColor(point1);
                var color2 = getLineColor(point2);

                if (color1 == null && color2 == null)
                    return;

                var argb1 = color1 ?? Color.Transparent;
                var argb2 = color2 ?? Color.Transparent;

                sprite.DrawQuad(
                    point1.NumericValue.WithZ(0), point2.NumericValue.WithZ(0),
                    (center + vector2 * offsetInner).NumericValue.WithZ(0),
                    (center + vector1 * offsetInner).NumericValue.WithZ(0),
                    new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    argb1, argb2, argb2, argb1
                    );
            });
        }
    }
}
