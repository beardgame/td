using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.Graphics.ImageSharp;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Textures;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Vertices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Custom;

sealed class MainMenuBackground(
    IndexedTrianglesMeshBuilder<UVColorVertex> meshBuilder,
    IEnumerable<IDisposable> disposables,
    IEnumerable<IRenderSetting> settings)
    : IDrawable, IDrawableTemplate
{
    private sealed record MaterialDrawableTemplate(Material Material) : IDrawableTemplate;

    public static MainMenuBackground GetOrCreate(Material material, IDrawableRenderers renderers)
    {
        var template = new MaterialDrawableTemplate(material);

        return renderers.GetOrCreateDrawableFor(
            template, material.Shader, DrawOrderGroup.UIBackground, -100, create);

        MainMenuBackground create()
        {
            var disposables = new List<IDisposable>();
            var settings = new List<IRenderSetting>();

            var i = 0;
            foreach (var (name, image) in material.Textures)
            {
                var texture = Texture.From(ImageTextureData.From(image),
                    c =>
                    {
                        c.SetFilterMode(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
                        c.SetWrapMode(TextureWrapMode.ClampToBorder, TextureWrapMode.ClampToBorder);
                        c.GenerateMipmap();
                    }
                );
                disposables.Add(texture);
                settings.Add(new TextureUniform(name, TextureUnit.Texture0 + i, texture));
                i++;
            }

            var meshBuilder = new IndexedTrianglesMeshBuilder<UVColorVertex>();
            disposables.Add(meshBuilder);

            return new MainMenuBackground(meshBuilder, disposables, settings);
        }
    }

    public void DrawRectangle(Vector3 topLeft, Vector2 size, Color color)
    {
        meshBuilder.AddQuad(
            new UVColorVertex(topLeft, new Vector2(0, 0), color),
            new UVColorVertex(topLeft + new Vector3(size.X, 0, 0), new Vector2(1, 0), color),
            new UVColorVertex(topLeft + new Vector3(size.X, size.Y, 0), new Vector2(1, 1), color),
            new UVColorVertex(topLeft + new Vector3(0, size.Y, 0), new Vector2(0, 1), color)
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

    public IRenderer CreateRendererWithSettings(IEnumerable<IRenderSetting> additionalSettings)
    {
        return Renderer.From(
            meshBuilder.ToRenderable(),
            settings.Concat(additionalSettings)
        );
    }
}
