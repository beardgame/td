using System;
using System.Collections.Immutable;
using Bearded.Graphics.RenderSettings;
using Bearded.TD.Content.Models;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering.Shapes;

using TextureUniformFactory = (Func<string, TextureUnit, IRenderSetting> Create, string Name);

sealed partial class ShapeDrawer
{
    private sealed class DrawableTemplate : IDrawableTemplate
    {
        public static DrawableTemplate Instance { get; } = new();
    }

    public static ShapeDrawer GetOrCreate(
        IDrawableRenderers renderers,
        Shader shader,
        RenderContext context,
        DrawOrderGroup drawGroup,
        int drawGroupOrderKey)
    {
        return renderers.GetOrCreateDrawableFor(
            DrawableTemplate.Instance, shader, drawGroup, drawGroupOrderKey,
            () => CreateUnregistered([
                (context.Renderers.Gradients.TextureUniform, "gradientBuffer"),
                (context.Renderers.ShapeComponents.TextureUniform, "componentBuffer"),
                (context.Compositor.IntermediateBlurTextureUniform, "intermediateBlurBackground"),
            ])
        );
    }

    public static ShapeDrawer CreateUnregistered(ReadOnlySpan<TextureUniformFactory> textures)
    {
        return new ShapeDrawer(build(textures));

        // TODO: a little experiment to see if we can make multiple texture uniforms more readable
        // consider creating a public helper somewhere?
        static ImmutableArray<IRenderSetting> build(ReadOnlySpan<TextureUniformFactory> factories)
        {
            var builder = ImmutableArray.CreateBuilder<IRenderSetting>(factories.Length);
            foreach (var factory in factories)
            {
                builder.Add(factory.Create(factory.Name, TextureUnit.Texture0 + builder.Count));
            }
            return builder.ToImmutable();
        }
    }
}
