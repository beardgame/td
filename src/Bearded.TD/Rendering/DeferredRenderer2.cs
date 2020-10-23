using System;
using System.Drawing;
using amulware.Graphics.Pipelines;
using amulware.Graphics.RenderSettings;
using amulware.Graphics.ShaderManagement;
using amulware.Graphics.Textures;
using Bearded.TD.UI.Layers;
using OpenToolkit.Graphics.OpenGL;
using OpenToolkit.Mathematics;

namespace Bearded.TD.Rendering
{
    using static amulware.Graphics.Pipelines.Context.BlendMode;
    using static amulware.Graphics.Pipelines.Context.DepthMode;
    using static Pipeline;
    using static Pipeline<DeferredRenderer2.RenderState>;

    class DeferredRenderer2
    {
        public class RenderState
        {
            public Vector2i Resolution { get; }
            public Vector2i LowResResolution { get; }

            public RenderTarget FinalRenderTarget { get; }
        }

        private Vector2i resolution;
        private Vector2i lowResResolution;
        private ShaderManager shaders;
        private SurfaceManager surfaces;
        private readonly Vector2Uniform levelUpSampleUVOffset = new Vector2Uniform("uvOffset");
        private readonly IPipeline<RenderState> pipeline;


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
                Resize(s => s.Resolution,
                    textures.DepthMask, textures.Diffuse, textures.Normal,
                    textures.Depth, textures.LightAccum, textures.Composition),
                Resize(s => s.LowResResolution,
                    textures.DepthMaskLowRes, textures.DiffuseLowRes,
                    textures.NormalLowRes, textures.DepthLowRes)
            );

            var renderedGBuffers = resizedBuffers.Then(InOrder(
                WithContext(
                    c => c.BindRenderTarget(targets.GeometryLowRes)
                        .SetViewport(s => new Rectangle(0, 0, s.LowResResolution.X, s.LowResResolution.Y))
                        .SetDepthMode(Default),
                    InOrder(ClearColor(), ClearDepth(), Do(s =>
                    {
                        s.Content.LevelRenderer.RenderAll();
                        /* contentSurfaces.LevelRenderer.RenderAll */
                    }))),
                upscale("deferred/copy", textures.DiffuseLowRes, targets.UpscaleDiffuse),
                upscale("deferred/copy", textures.NormalLowRes, targets.UpscaleNormal),
                upscale("deferred/copy", textures.DepthLowRes, targets.UpscaleDepth),
                WithContext(
                    c => c.SetDepthMode(WriteOnly),
                    upscale("deferred/copyDepth", textures.DepthMaskLowRes, targets.UpscaleDepthMask)),
                WithContext(
                    c => c.BindRenderTarget(targets.Geometry)
                        .SetDepthMode(Default)
                        .SetBlendMode(Premultiplied),
                    Do(s =>
                    {
                        s.Content.WorldDrawGroups.RenderAll();
                        /* renderDrawGroups(contentSurfaces, worldDrawGroups) */
                    }))
            ));

            var compositedFrame = renderedGBuffers.Then(InOrder(
                WithContext(
                    c => c.BindRenderTarget(targets.LightAccum)
                        .SetDepthMode(TestOnly(DepthFunction.Less))
                        .SetBlendMode(Premultiplied),
                    InOrder(
                        ClearColor(),
                        Render(surfaces.PointLightRenderer),
                        Render(surfaces.SpotLightRenderer)
                    )),
                WithContext(
                    c => c.BindRenderTarget(targets.Composition),
                    InOrder(
                        PostProcess(getShader("deferred/compose"), out _,
                            // TODO: it should be much easier to quickly pass in a couple of textures
                            new TextureUniform("albedoTexture", TextureUnit.Texture0, textures.Diffuse.Texture),
                            new TextureUniform("lightTexture", TextureUnit.Texture1, textures.LightAccum.Texture)
                        ),
                        WithContext(
                            c => c.SetDepthMode(TestOnly(DepthFunction.Less)),
                            InOrder(
                                Render(contentSurfaces.fluids)
                                /* renderDrawGroups(contentSurfaces, postLightGroups) */
                            ))
                    ))
            ));


            var fullRender = compositedFrame.Then(
                WithContext(
                    c => c.BindRenderTarget(s => s.FinalRenderTarget),
                    PostProcess(getShader("deferred/copy"), out _,
                        new TextureUniform("inputTexture", TextureUnit.Texture0, textures.Composition.Texture),
                        new Vector2Uniform("uvOffset"))
                    )
            );

            pipeline = fullRender;


            // TODO: it would be neat to have some steps have a more semantic associated output
            // for example, WithContext(c => c.BindRenderTarget(target)) could be replaced
            // by a call that takes texture definitions, and returns a pipeline step that also
            // exposes the PipeLineTexture objects which can then be used as input for future steps
            // and that would possibly allow us to write things more like a tree of actual dependencies
            // instead of of a linear chain of commands
        }

        public void Render(IDeferredRenderLayer deferredLayer, RenderTarget target)
        {
            // prepare RenderState with resolutions, final render target (need a c.BindRenderTarget(Func<TState..>), and content

            var state = new RenderState();

            pipeline.Execute(state);

            // cleaning of mesh builders will happen in game renderer
        }

        private IPipeline<RenderState> upscale(string shaderName, PipelineTextureBase texture, PipelineRenderTarget target)
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
