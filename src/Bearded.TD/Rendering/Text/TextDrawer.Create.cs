using System;
using System.Collections.Generic;
using Bearded.Graphics.ImageSharp;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Textures;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Models.Fonts;
using Bearded.TD.Rendering.Vertices;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering.Text;

static class TextDrawer
{
    public static TextDrawer<TVertex, TVertexParameters> Create<TVertex, TVertexParameters>(
        Font font, TextDrawerConfiguration config,
        CreateVertex<TVertex, TVertexParameters> createVertex)
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
            var texture = Texture.From(ImageTextureData.From(image), c =>
                {
                    c.SetFilterMode(TextureMinFilter.Linear, TextureMagFilter.Linear);
                }
            );
            disposables.Add(texture);
            settings.Add(new TextureUniform(name, TextureUnit.Texture0 + i, texture));
            i++;
        }

        var meshBuilder = new ExpandingIndexedTrianglesMeshBuilder<TVertex>();
        disposables.Add(meshBuilder);
        settings.Add(new Vector2Uniform("unitRange", font.Definition.UnitRange));

        return new TextDrawer<TVertex, TVertexParameters>(
            font.Definition,
            settings,
            meshBuilder,
            createVertex,
            disposables,
            config
        );
    }
}
