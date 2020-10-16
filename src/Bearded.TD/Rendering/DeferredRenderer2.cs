using System;
using amulware.Graphics.Pipelines;
using amulware.Graphics.RenderSettings;
using amulware.Graphics.ShaderManagement;
using OpenToolkit.Graphics.OpenGL;
using OpenToolkit.Mathematics;
using static amulware.Graphics.Pipelines.Context.BlendMode;
using static amulware.Graphics.Pipelines.Context.DepthMode;
using static amulware.Graphics.Pipelines.Pipeline;

namespace Bearded.TD.Rendering
{
    // NEXT TIME: work on content loading instead of this - the rest here should be easy (RIP)

    public class DeferredRenderer2
    {
        private Vector2i resolution;
        private Vector2i lowResResolution;
        private ShaderManager shaders;
        private readonly Vector2Uniform levelUpSampleUVOffset = new Vector2Uniform("uvOffset");

        public DeferredRenderer2()
        {
            var textures = new
            {
                DiffuseLowRes = Texture(PixelInternalFormat.Rgba), // rgba
                NormalLowRes = Texture(PixelInternalFormat.Rgba), // xyz
                DepthLowRes = Texture(PixelInternalFormat.R16f), // z (0-1, camera space)
                DepthMaskLowRes = DepthTexture(PixelInternalFormat.DepthComponent32f),

                Diffuse = Texture(PixelInternalFormat.Rgba), // rgba
                Normal = Texture(PixelInternalFormat.Rgba), // xyz
                Depth = Texture(PixelInternalFormat.R16f), // z (0-1, camera space)
                DepthMask = DepthTexture(PixelInternalFormat.DepthComponent32f),

                LightAccum = Texture(PixelInternalFormat.Rgb),
                Composition = Texture(PixelInternalFormat.Rgba),
            };

            var targets = new
            {
                GeometryLowRes = RenderTargetWithDepthAndColors(
                    textures.DepthMaskLowRes, textures.DiffuseLowRes, textures.NormalLowRes, textures.DepthLowRes),
                Geometry = RenderTargetWithDepthAndColors(
                    textures.DepthMask, textures.Diffuse, textures.Normal, textures.Depth),
                LightAccum = RenderTargetWithColors(textures.LightAccum),
                Composition = RenderTargetWithDepthAndColors(textures.DepthMask, textures.Composition),

                UpscaleDiffuse = RenderTargetWithColors(textures.Diffuse),
                UpscaleNormal = RenderTargetWithColors(textures.Normal),
                UpscaleDepth = RenderTargetWithColors(textures.Depth),
                UpscaleDepthMask = RenderTargetWithDepthAndColors(textures.DepthMask),
            };


            // resizeForCameraDistance
            // contentSurfaces.LevelRenderer.PrepareForRender

            var resizedBuffers = InOrder(
                Resize(() => resolution,
                    textures.DepthMask, textures.Diffuse, textures.Normal,
                    textures.Depth, textures.LightAccum, textures.Composition),
                Resize(() => lowResResolution,
                    textures.DepthMaskLowRes, textures.DiffuseLowRes,
                    textures.NormalLowRes, textures.DepthLowRes)
            );

            var renderedGBuffers = resizedBuffers.Then(InOrder(
                WithContext(
                    c => c.SetDepthMode(Default).BindRenderTarget(targets.GeometryLowRes),
                    InOrder(ClearColor(), ClearDepth(), Do(() =>
                    {
                        /* contentSurfaces.LevelRenderer.RenderAll */
                    }))),
                upscale("deferred/copy", textures.DiffuseLowRes, targets.UpscaleDiffuse),
                upscale("deferred/copy", textures.NormalLowRes, targets.UpscaleNormal),
                upscale("deferred/copy", textures.DepthLowRes, targets.UpscaleDepth),
                WithContext(
                    c => c.SetDepthMode(WriteOnly),
                    upscale("deferred/copyDepth", textures.DepthMaskLowRes, targets.UpscaleDepthMask)
                ),
                WithContext(
                    c => c.SetDepthMode(Default).SetBlendMode(Premultiplied).BindRenderTarget(targets.Geometry),
                    Do(() =>
                    {
                        /* renderDrawGroups(contentSurfaces, worldDrawGroups) */
                    }))
                ));
        }

        private IPipeline upscale(string shaderName, PipelineTextureBase texture, PipelineRenderTarget target)
        {
            return WithContext(
                c => c.BindRenderTarget(target),
                PostProcess(getShader(shaderName), out _,
                    new TextureUniform("inputTexture", TextureUnit.Texture0,
                        texture.Texture),
                    levelUpSampleUVOffset)
            );
        }

        private IRendererShader getShader(string name)
        {
            if (shaders.TryGetRendererShader(name, out var shader))
            {
                return shader;
            }

            throw new ArgumentException("Shader with name not found.", nameof(name));
        }

        private void setLevelViewMatrix()
        {
            throw new System.NotImplementedException();
        }
    }
}
