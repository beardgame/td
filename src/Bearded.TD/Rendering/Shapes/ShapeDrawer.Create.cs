using System;
using System.Collections.Immutable;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.RenderSettings;
using Bearded.TD.Content.Models;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

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
                (context.DeferredRenderer.GetDepthBufferUniform, "depthBuffer"),
            ])
        );
    }

    public static ShapeDrawer CreateUnregistered(
        ReadOnlySpan<TextureUniformFactory> textures,
        Func<IIndexedTrianglesMeshBuilder<ShapeVertex, ushort>, IMeshBuilder>? meshBuilderFactory = null)
    {
        return new ShapeDrawer(meshBuilderFactory, build(textures));

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

    public interface IMeshBuilder
    {
        void AddQuad(
            float x0, float x1, float y0, float y1, float z,
            ShapeVertex.ShapeComponents components,
            ShapeData shape
            );
    }

    private sealed class DefaultMeshBuilder(IIndexedTrianglesMeshBuilder<ShapeVertex, ushort> mesh)
        : IMeshBuilder
    {
        public void AddQuad(
            float x0, float x1, float y0, float y1, float z,
            ShapeVertex.ShapeComponents components,
            ShapeData shape
            )
        {
            mesh.AddQuad(
                new ShapeVertex(new Vector3(x0, y0, z), shape, components),
                new ShapeVertex(new Vector3(x1, y0, z), shape, components),
                new ShapeVertex(new Vector3(x1, y1, z), shape, components),
                new ShapeVertex(new Vector3(x0, y1, z), shape, components)
            );
        }
    }
}
