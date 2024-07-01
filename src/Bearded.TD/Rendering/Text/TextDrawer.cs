using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Text;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Models.Fonts;
using Bearded.TD.Rendering.Vertices;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Text;

readonly record struct TextDrawerConfiguration(
    Vector2? AlignGrid = null,
    Vector2 Offset = default,
    bool IgnoreKerning = false);

sealed class TextDrawer<TVertex, TVertexParameters>(
    IFontDefinition font,
    IEnumerable<IRenderSetting> settings,
    ExpandingIndexedTrianglesMeshBuilder<TVertex> meshBuilder,
    CreateVertex<TVertex, TVertexParameters> createTextVertex,
    IEnumerable<IDisposable> disposables,
    TextDrawerConfiguration config)
    : ITextDrawer<TVertexParameters>, IDrawable
    where TVertex : struct, IVertexData
{
    private readonly TextLayout.Config layoutConfig = new(IgnoreKerning: config.IgnoreKerning);

    public void DrawLine(
        Vector3 xyz, string text,
        float fontHeight, float alignHorizontal, float alignVertical,
        Vector3 unitRightDP, Vector3 unitDownDP, TVertexParameters parameters)
    {
        Span<LaidOutGlyph> glyphs = stackalloc LaidOutGlyph[text.Length];
        var line = TextLayout.LayoutLine(text, font, glyphs, layoutConfig);

        DrawLine(line, xyz, fontHeight, alignHorizontal, alignVertical, unitRightDP, unitDownDP, parameters);
    }

    public void DrawLine(
        GlyphLine line, Vector3 xyz,
        float fontHeight, float alignHorizontal, float alignVertical,
        Vector3 unitRightDP, Vector3 unitDownDP, TVertexParameters parameters)
    {
        var unitX = unitRightDP * fontHeight;
        var unitY = unitDownDP * -fontHeight; // negated, because unitY points UP, relative to font
        var alignOffset = new Vector2(
            -alignHorizontal * line.Width,
            alignVertical - 0.5f - font.CapHeight / 2 // center text around middle of cap height
        );
        var origin = xyz + transform(alignOffset, unitX, unitY);

        DrawLine(line, origin, unitX, unitY, parameters);
    }

    public void DrawLine(GlyphLine line, Vector3 origin, Vector3 unitX, Vector3 unitY, TVertexParameters parameters)
    {
        var glyphs = line.Glyphs;
        meshBuilder.Add(
            glyphs.Length * 4, glyphs.Length * 6,
            out var vertices, out var indices, out var indexOffset);

        var vertex = 0;
        var index = 0;
        var vertexIndex = indexOffset;

        // TODO: this should consider ui scaling, or the config needs to scale with it
        if (config.AlignGrid is { } align)
        {
            var cell = origin.Xy / align;
            origin.Xy = new Vector2((int)cell.X, (int)cell.Y) * align;
        }

        origin.Xy += config.Offset;

        var advCorrection = 0f;

        foreach (ref var glyph in glyphs)
        {
            var (xy, uv, adv) = glyph;

            if (config.AlignGrid is { X: var alignX })
            {
                alignX /= unitX.X;
                var cell = (adv + advCorrection) / alignX;
                var newAdv = (int)(cell + 0.5f) * alignX;

                advCorrection = newAdv - adv;

                xy = xy.TranslateX(advCorrection);
            }

            var bottomLeft = origin + transform(xy.BottomLeft, unitX, unitY);
            var stepRight = xy.Width * unitX;
            var stepUp = xy.Height * unitY;

            vertices[vertex] = createTextVertex(bottomLeft, uv.BottomLeft, parameters);
            vertices[vertex + 1] = createTextVertex(bottomLeft + stepUp, uv.TopLeft, parameters);
            vertices[vertex + 2] = createTextVertex(bottomLeft + stepRight, uv.BottomRight, parameters);
            vertices[vertex + 3] = createTextVertex(bottomLeft + stepUp + stepRight, uv.TopRight, parameters);

            indices[index] = vertexIndex;
            indices[index + 1] = (ushort)(vertexIndex + 1);
            indices[index + 2] = (ushort)(vertexIndex + 2);
            indices[index + 3] = (ushort)(vertexIndex + 1);
            indices[index + 4] = (ushort)(vertexIndex + 3);
            indices[index + 5] = (ushort)(vertexIndex + 2);

            vertex += 4;
            index += 6;
            vertexIndex += 4;
        }
    }

    public (Vector3 Width, Vector3 Height) StringSize(
        string text, float fontHeight, Vector3 unitRightDP, Vector3 unitDownDP)
        => (StringWidth(text, fontHeight, unitRightDP), StringHeight(fontHeight, unitDownDP));

    public Vector3 StringWidth(string text, float fontHeight, Vector3 unitRightDP)
        => TextLayout.LineWidth(text, font, layoutConfig) * fontHeight * unitRightDP;

    public Vector3 StringHeight(float fontHeight, Vector3 unitDownDP)
        => fontHeight * unitDownDP;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector3 transform(Vector2 v, Vector3 unitX, Vector3 unitY)
        => v.X * unitX + v.Y * unitY;

    public IRenderer CreateRendererWithSettings(IEnumerable<IRenderSetting> additionalSettings)
    {
        return BatchedRenderer.From(
            meshBuilder.ToRenderable(),
            settings.Concat(additionalSettings)
        );
    }

    public void Clear()
    {
        meshBuilder.Clear();
    }

    public void Dispose()
    {
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }
}
