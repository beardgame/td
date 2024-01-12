using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Bearded.Graphics.ImageSharp;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Text;
using Bearded.Graphics.Textures;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Models.Fonts;
using Bearded.TD.Rendering.Vertices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Font = Bearded.TD.Content.Models.Fonts.Font;

namespace Bearded.TD.Rendering.Text;

static class TextDrawer
{
    public static TextDrawer<TVertex, TVertexParameters> Create<TVertex, TVertexParameters>(
        Font font,
        CreateVertex<TVertex, TVertexParameters> createVertex,
        Shader? shader = null)
        where TVertex : struct, IVertexData
    {
        var disposables = new List<IDisposable>(font.Material.Textures.Count + 1);
        var settings = new List<IRenderSetting>(font.Material.Textures.Count + 1);

        var i = 0;
        foreach (var (name, image) in font.Material.Textures)
        {
            // TODO: these textures should be cached
            // - cache in font, similar to how textures are cached in PackedSpriteSet?
            // - could still cause duplication if same material is used for different purposes...
            var texture = Texture.From(ImageTextureData.From(image), c => c.GenerateMipmap());
            disposables.Add(texture);
            settings.Add(new TextureUniform(name, TextureUnit.Texture0 + i, texture));
            i++;
        }

        var meshBuilder = new ExpandingIndexedTrianglesMeshBuilder<TVertex>();
        disposables.Add(meshBuilder);
        settings.Add(new Vector2Uniform("unitRange", font.Definition.UnitRange));

        shader ??= font.Material.Shader;

        return new TextDrawer<TVertex, TVertexParameters>(
            font.Definition,
            settings,
            shader,
            meshBuilder,
            createVertex,
            disposables
            );
    }
}

sealed class TextDrawer<TVertex, TVertexParameters>(
    IFontDefinition font,
    IEnumerable<IRenderSetting> settings,
    Shader shader,
    ExpandingIndexedTrianglesMeshBuilder<TVertex> meshBuilder,
    CreateVertex<TVertex, TVertexParameters> createTextVertex,
    IEnumerable<IDisposable> disposables)
    : ITextDrawer<TVertexParameters>, IDrawable
    where TVertex : struct, IVertexData
{
    public void DrawLine(
        Vector3 xyz, string text,
        float fontHeight, float alignHorizontal, float alignVertical,
        Vector3 unitRightDP, Vector3 unitDownDP, TVertexParameters parameters)
    {
        Span<LaidOutGlyph> glyphs = stackalloc LaidOutGlyph[text.Length];
        var line = TextLayout.LayoutLine(text, font, glyphs);

        DrawLine(line, xyz, fontHeight, alignHorizontal, alignVertical, unitRightDP, unitDownDP, parameters);
    }

    public void DrawLine(
        GlyphLine line, Vector3 xyz,
        float fontHeight, float alignHorizontal, float alignVertical,
        Vector3 unitRightDP, Vector3 unitDownDP, TVertexParameters parameters)
    {
        var unitX = unitRightDP * fontHeight;
        var unitY = unitDownDP * fontHeight;
        var alignOffset = new Vector2(
            -alignHorizontal * line.Width,
            -alignVertical
        );
        var origin = xyz + transform(alignOffset, unitX, unitY);

        var glyphs = line.Glyphs;
        meshBuilder.Add(
            glyphs.Length * 4, glyphs.Length * 6,
            out var vertices, out var indices, out var indexOffset);

        var vertex = 0;
        var index = 0;
        var vertexIndex = indexOffset;

        foreach (ref var glyph in glyphs)
        {
            var (xy, uv) = glyph;

            var topLeft = origin + transform(xy.TopLeft, unitX, unitY);
            var stepRight = xy.Width * unitX;
            var stepDown = xy.Height * unitY;

            vertices[vertex] = createTextVertex(topLeft, uv.TopLeft, parameters);
            vertices[vertex + 1] = createTextVertex(topLeft + stepRight, uv.TopRight, parameters);
            vertices[vertex + 2] = createTextVertex(topLeft + stepDown, uv.BottomLeft, parameters);
            vertices[vertex + 3] = createTextVertex(topLeft + stepRight + stepDown, uv.BottomRight, parameters);

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
        => TextLayout.LineWidth(text, font) * fontHeight * unitRightDP;

    public Vector3 StringHeight(float fontHeight, Vector3 unitDownDP)
        => fontHeight * unitDownDP;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector3 transform(Vector2 v, Vector3 unitX, Vector3 unitY)
        => v.X * unitX + v.Y * unitY;

    public IRenderer CreateRendererWithSettings(IEnumerable<IRenderSetting> additionalSettings)
    {
        var renderer = BatchedRenderer.From(
            meshBuilder.ToRenderable(),
            settings.Concat(additionalSettings)
        );

        shader.RendererShader.UseOnRenderer(renderer);

        return renderer;
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
