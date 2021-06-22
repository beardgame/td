using System;
using System.Drawing;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Pipelines;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Textures;
using Bearded.TD.Content.Models;
using Bearded.TD.Meta;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using static Bearded.Graphics.Pipelines.Context.CullMode;
using static Bearded.TD.Content.Models.SpriteDrawGroup;

namespace Bearded.TD.Rendering
{
    using static Graphics.Pipelines.Context.BlendMode;
    using static Graphics.Pipelines.Context.DepthMode;
    using static Pipeline;
    using static Pipeline<DeferredRenderer.RenderState>;

    class DeferredRenderer
    {
        private static readonly SpriteDrawGroup[] worldLowResDrawGroups = { LowResLevelDetail };
        private static readonly SpriteDrawGroup[] worldDrawGroups = { LevelDetail, Building, Unit };
        private static readonly SpriteDrawGroup[] postLightGroups = { Particle, Unknown };

        public class RenderState
        {
            public Vector2i Resolution { get; }
            public Vector2i LowResResolution { get; }

            public RenderTarget FinalRenderTarget { get; }

            public ContentRenderers Content { get; }

            public RenderState(Vector2i resolution, Vector2i lowResResolution, RenderTarget finalRenderTarget, ContentRenderers content)
            {
                Resolution = resolution;
                LowResResolution = lowResResolution;
                FinalRenderTarget = finalRenderTarget;
                Content = content;
            }
        }

        private readonly CoreShaders shaders;
        private readonly CoreRenderSettings settings;
        private readonly Vector2Uniform levelUpSampleUVOffset = new("uvOffset");
        private readonly Vector2Uniform gBufferResolution = new("resolution");
        private readonly FloatUniform hexagonalFallOffDistance = new("hexagonalFallOffDistance");
        private readonly IPipeline<RenderState> pipeline;

        private ViewportSize viewport;

        public IRenderSetting DepthBuffer { get; }

        public ExpandingIndexedTrianglesMeshBuilder<PointLightVertex> PointLights { get; } = new();
        public ExpandingIndexedTrianglesMeshBuilder<SpotlightVertex> Spotlights { get; } = new();

        public DeferredRenderer(
            CoreRenderSettings settings,
            CoreShaders coreShaders)
        {
            this.settings = settings;
            shaders = coreShaders;

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

            DepthBuffer = new TextureUniform("depthBuffer", TextureUnit.Texture1, textures.Depth.Texture);

            var (pointLightRenderer, spotLightRenderer) = setupLightRenderers(textures.Normal);


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
                        Do(s => s.Content.LevelRenderer.RenderAll()),
                        Do(s => worldLowResDrawGroups.ForEach(s.Content.RenderDrawGroup))
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
                        Render(pointLightRenderer, spotLightRenderer)
                    )),
                WithContext(
                    c => c.BindRenderTarget(targets.Composition),
                    InOrder(
                        PostProcess(shaders.GetShaderProgram("deferred/compose"), out _,
                            // TODO: it should be much easier to quickly pass in a couple of textures
                            new TextureUniform("albedoTexture", TextureUnit.Texture0, textures.Diffuse.Texture),
                            new TextureUniform("lightTexture", TextureUnit.Texture1, textures.LightAccum.Texture),
                            new TextureUniform("depthBuffer", TextureUnit.Texture2, textures.Depth.Texture),
                            settings.FarPlaneBaseCorner, settings.FarPlaneUnitX, settings.FarPlaneUnitY, settings.CameraPosition,
                            hexagonalFallOffDistance
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
                    PostProcess(shaders.GetShaderProgram("deferred/copy"), out _,
                        new TextureUniform("inputTexture", TextureUnit.Texture0, textures.Composition.Texture),
                        new Vector2Uniform("uvOffset", Vector2.Zero)
                        )
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

        private (IRenderer pointLightRenderer, IRenderer spotLightRenderer) setupLightRenderers(
            PipelineTexture normalBuffer)
        {
            var neededSettings = new[]
            {
                settings.ViewMatrix, settings.ProjectionMatrix,
                settings.FarPlaneBaseCorner, settings.FarPlaneUnitX, settings.FarPlaneUnitY, settings.CameraPosition,
                new TextureUniform("normalBuffer", TextureUnit.Texture0, normalBuffer.Texture),
                DepthBuffer, gBufferResolution
            };

            var pointLight = BatchedRenderer.From(PointLights.ToRenderable(), neededSettings);
            shaders.GetShaderProgram("deferred/pointlight").UseOnRenderer(pointLight);

            var spotLight = BatchedRenderer.From(Spotlights.ToRenderable(), neededSettings);
            shaders.GetShaderProgram("deferred/spotlight").UseOnRenderer(spotLight);

            return (pointLight, spotLight);
        }

        public void RenderLayer(IDeferredRenderLayer deferredLayer, RenderTarget target)
        {
            var (lowResResolution, resolution) = resizeForCameraDistance(deferredLayer.CameraDistance);
            gBufferResolution.Value = new Vector2(resolution.X, resolution.Y);
            hexagonalFallOffDistance.Value = deferredLayer.HexagonalFallOffDistance;

            deferredLayer.ContentRenderers.LevelRenderer.PrepareForRender();

            var state = new RenderState(resolution, lowResResolution, target, deferredLayer.ContentRenderers);

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

            var viewMatrix = settings.ViewMatrix.Value;
            var translation = viewMatrix.Row3;

            var levelTranslation = new Vector2(
                MoreMath.RoundToInt(translation.X / pixelStep) * pixelStep,
                MoreMath.RoundToInt(translation.Y / pixelStep) * pixelStep
            );

            viewMatrix.Row3.Xy = levelTranslation;
            settings.ViewMatrixLevel.Value = viewMatrix;

            var offset = (levelTranslation - translation.Xy) / (cameraDistance * 2);
            offset.X = offset.X / viewport.Width * viewport.Height;
            levelUpSampleUVOffset.Value = offset;
        }

        private IPipeline<RenderState> upscale(string shaderName, PipelineTextureBase texture, PipelineRenderTarget target)
        {
            return WithContext(
                c => c.BindRenderTarget(target),
                PostProcess(shaders.GetShaderProgram(shaderName), out _,
                    new TextureUniform("inputTexture", TextureUnit.Texture0, texture.Texture),
                    levelUpSampleUVOffset
                    )
            );
        }


        public void ClearAll()
        {
            PointLights.Clear();
            Spotlights.Clear();
        }
    }
}
