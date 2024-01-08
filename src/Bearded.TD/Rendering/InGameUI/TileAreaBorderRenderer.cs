using System;
using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.InGameUI;

static class TileAreaBorderRenderer
{
    public static void Render(TileAreaBorder border, GameState game,
        Color color, float lineWidth = 0.3f)
    {
        Render(border, game, _ => color, lineWidth);
    }

    public static void Render(TileAreaBorder border, GameState game,
        Func<Position2, Color?> getLineColor, float lineWidth = 0.3f)
    {
        // TODO: This should not be hard coded
        var sprites = game.Meta.Blueprints.Sprites[ModAwareId.ForDefaultMod("particle")];
        var sprite = sprites.GetSprite("halo").MakeConcreteWith(
            game.Meta.SpriteRenderers, DrawOrderGroup.IgnoreDepth, 0, UVColorVertex.Create,
            game.Meta.Blueprints.Shaders[ModAwareId.ForDefaultMod("default-sprite")]);

        Render(border, sprite, getLineColor, lineWidth);
    }

    public static void Render(TileAreaBorder border, CoreDrawers drawers,
        Color color, float lineWidth = 0.3f)
    {
        Render(border, drawers, _ => color, lineWidth);
    }

    public static void Render(TileAreaBorder border, CoreDrawers drawers,
        Func<Position2, Color?> getLineColor, float lineWidth = 0.3f)
    {
        Render(border, drawers.CustomPrimitives, getLineColor, lineWidth);
    }

    public static void Render(TileAreaBorder border, IDrawableSprite<Color> sprite,
        Color color, float lineWidth = 0.3f)
    {
        Render(border, sprite, _ => color, lineWidth);
    }

    public static void Render(
        TileAreaBorder border,
        IDrawableSprite<Color> sprite,
        Func<Position2, Color?> getLineColor,
        float lineWidth = 0.3f)
    {
        const float z = 0.2f;

        var offsetOuter = new Unit(Constants.Game.World.HexagonSide);
        var lineWidthU = new Unit(lineWidth);

        border.Visit(t =>
        {
            var (tile, direction, beforeIsConvex, afterIsConvex) = t;

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

            var v1 = beforeIsConvex ? vector1 : vector2;
            var v2 = afterIsConvex ? vector2 : vector1;

            sprite.DrawQuad(
                point1.NumericValue.WithZ(z), point2.NumericValue.WithZ(z),
                (point2 - v2 * lineWidthU).NumericValue.WithZ(z),
                (point1 - v1 * lineWidthU).NumericValue.WithZ(z),
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                argb1, argb2, argb2, argb1
            );
        });
    }
}
