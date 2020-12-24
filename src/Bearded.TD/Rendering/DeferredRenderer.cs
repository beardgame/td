using System;
using System.Drawing;
using amulware.Graphics.Pipelines;
using amulware.Graphics.RenderSettings;
using amulware.Graphics.ShaderManagement;
using amulware.Graphics.Textures;
using Bearded.TD.Content.Models;
using Bearded.TD.Meta;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using static amulware.Graphics.Pipelines.Context.CullMode;
using static Bearded.TD.Content.Models.SpriteDrawGroup;

namespace Bearded.TD.Rendering
{
    using static amulware.Graphics.Pipelines.Context.BlendMode;
    using static amulware.Graphics.Pipelines.Context.DepthMode;
    using static Pipeline;
    using static Pipeline<DeferredRenderer.RenderState>;

    class DeferredRenderer
    {
        private static readonly SpriteDrawGroup[] worldDrawGroups = { LevelDetail, Building, Unit };
        private static readonly SpriteDrawGroup[] postLightGroups = { Particle, Unknown };

        public class RenderState
        {
            public Vector2i Resolution { get; }
            public Vector2i LowResResolution { get; }

            public RenderTarget FinalRenderTarget { get; }

            public ContentSurfaceManager Content { get; }

            public RenderState(Vector2i resolution, Vector2i lowResResolution, RenderTarget finalRenderTarget, ContentSurfaceManager content)
            {
                Resolution = resolution;
                LowResResolution = lowResResolution;
                FinalRenderTarget = finalRenderTarget;
                Content = content;
            }
        }

        private readonly ShaderManager shaders;
        private readonly SurfaceManager surfaces;
        private readonly Vector2Uniform levelUpSampleUVOffset = new Vector2Uniform("uvOffset");
        private readonly Vector2Uniform gBufferResolution = new Vector2Uniform("resolution");
        private readonly IPipeline<RenderState> pipeline;

        private ViewportSize viewport;

        public DeferredRenderer(SurfaceManager surfaceManager)
        {
            surfaces = surfaceManager;
            shaders = surfaceManager.Shaders;

            void upscaleNearest(Texture.Target t)
            {
                t.SetFilterMode(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
                t.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            }

            var textures = new
            {
                DiffuseLowRes = Texture(PixelInternalFormat.Rgba, setup: upscaleNearest), // rgba
                NormalLowRes = Texture(PixelInternalFormat.Rgba, setup: upscaleNearest), // xyz
                DepthLowRes = Texture(PixelInternalFormat.R16f, setup: upscaleNearest), // z (0-1, camera space)
                DepthMaskLowRes = DepthTexture(PixelInternalFormat.DepthComponent32f, setup: upscaleNearest),

                Diffuse = Texture(PixelInternalFormat.Rgba), // rgba
                Normal = Texture(PixelInternalFormat.Rgba), // xyz
                Depth = Texture(PixelInternalFormat.R16f), // z (0-1, camera space)
                DepthMask = DepthTexture(PixelInternalFormat.DepthComponent32f),

                LightAccum = Texture(PixelInternalFormat.Rgb),
                Composition = Texture(PixelInternalFormat.Rgba),
            };

            // TODO: this injection is ugly and fragile, let's find a better way
            surfaceManager.InjectDeferredBuffer(
                textures.Normal.Texture,
                textures.Depth.Texture,
                gBufferResolution);

            var targets = new
            {
                GeometryLowRes = RenderTargetWithDepthAndColors(
                    textures.DepthMaskLowRes, textures.DiffuseLowRes, textures.NormalLowRes, textures.DepthLowRes),
                Geometry = RenderTargetWithDepthAndColors(
                    textures.DepthMask, textures.Diffuse, textures.Normal, textures.Depth),
                LightAccum = RenderTargetWithDepthAndColors(textures.DepthMask, textures.LightAccum),
                Composition = RenderTargetWithDepthAndColors(textures.DepthMask, textures.Composition),

                UpscaleDiffuse = RenderTargetWithColors(textures.Diffuse),
                UpscaleNormal = RenderTargetWithColors(textures.Normal),
                UpscaleDepth = RenderTargetWithColors(textures.Depth),
                UpscaleDepthMask = RenderTargetWithDepthAndColors(textures.DepthMask),
            };


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
                    InOrder(
                        ClearColor(),
                        ClearDepth(),
                        Do(s => s.Content.LevelRenderer.RenderAll())
                        )),
                // TODO: if low and regular resolution are same, render level to regular target directly and skip upscaling
                // compositing two different pipelines with most steps shared should be easy
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
                    Do(s => worldDrawGroups.ForEach(s.Content.RenderDrawGroup)))
            ));

            var compositedFrame = renderedGBuffers.Then(InOrder(
                WithContext(
                    c => c.BindRenderTarget(targets.LightAccum)
                        .SetDepthMode(TestOnly(DepthFunction.Gequal))
                        .SetBlendMode(Premultiplied)
                        .SetCullMode(RenderBack),
                    InOrder(
                        ClearColor(),
                        Render(surfaces.PointLightRenderer, surfaces.SpotLightRenderer)
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
                            c => c.SetDepthMode(TestOnly(DepthFunction.Less))
                                .SetBlendMode(Premultiplied),
                            InOrder(
                                Do(s => s.Content.FluidGeometries.ForEach(f => f.Render())),
                                Do(s => postLightGroups.ForEach(s.Content.RenderDrawGroup))
                            ))
                    ))
            ));


            var fullRender = compositedFrame.Then(
                WithContext(
                    c => c.BindRenderTarget(s => s.FinalRenderTarget),
                    PostProcess(getShader("deferred/copy"), out _,
                        new TextureUniform("inputTexture", TextureUnit.Texture0, textures.Composition.Texture),
                        levelUpSampleUVOffset)
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

        public void RenderLayer(IDeferredRenderLayer deferredLayer, RenderTarget target)
        {
            var (lowResResolution, resolution) = resizeForCameraDistance(deferredLayer.CameraDistance);
            gBufferResolution.Value = new Vector2(resolution.X, resolution.Y);

            deferredLayer.DeferredSurfaces.LevelRenderer.PrepareForRender();

            var state = new RenderState(resolution, lowResResolution, target, deferredLayer.DeferredSurfaces);

            pipeline.Execute(state);

            // cleaning of mesh builders will happen in game renderer
        }

        public void OnResize(ViewportSize newViewport)
        {
            viewport = newViewport;
        }

        private (Vector2i LowResResolution, Vector2i Resolution) resizeForCameraDistance(float cameraDistance)
        {
            var lowResResolution = calculateResolution(cameraDistance, Constants.Rendering.PixelsPerTileLevelResolution);
            var resolution = calculateResolution(cameraDistance, Constants.Rendering.PixelsPerTileCompositeResolution);

            setLevelViewMatrix(cameraDistance);

            return (lowResResolution, resolution);
        }

        private Vector2i calculateResolution(float cameraDistance, float pixelsPerTile)
        {
            var screenPixelsPerTile = viewport.Height * 0.5f / cameraDistance;

            var scale = Math.Min(1, pixelsPerTile / screenPixelsPerTile);

            var bufferSizeFactor = UserSettings.Instance.Graphics.SuperSample;

            var w = (int) (viewport.Width * bufferSizeFactor * scale);
            var h = (int) (viewport.Height * bufferSizeFactor * scale);

            return (w, h);
        }

        private void setLevelViewMatrix(float cameraDistance)
        {
            var pixelStep = 1f / Constants.Rendering.PixelsPerTileLevelResolution;

            var viewMatrix = surfaces.ViewMatrix.Value;
            var translation = viewMatrix.Row3;

            var levelTranslation = new Vector2(
                Mathf.RoundToInt(translation.X / pixelStep) * pixelStep,
                Mathf.RoundToInt(translation.Y / pixelStep) * pixelStep
            );

            viewMatrix.Row3.Xy = levelTranslation;
            surfaces.ViewMatrixLevel.Value = viewMatrix;

            var offset = (levelTranslation - translation.Xy) / (cameraDistance * 2);
            offset.X = offset.X / viewport.Width * viewport.Height;
            levelUpSampleUVOffset.Value = offset;
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
    }
}
