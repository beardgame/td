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
using Bearded.TD.Rendering.Deferred.Level;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using static Bearded.Graphics.Pipelines.Context.CullMode;
using static Bearded.TD.Content.Models.SpriteDrawGroup;

namespace Bearded.TD.Rendering;

using static Graphics.Pipelines.Context.BlendMode;
using static Graphics.Pipelines.Context.DepthMode;
using static Pipeline;
using static Pipeline<DeferredRenderer.RenderState>;

class DeferredRenderer
{
    private static readonly SpriteDrawGroup[] worldLowResDrawGroups = { LowResLevelDetail };
    private static readonly SpriteDrawGroup[] worldDrawGroups = { LevelDetail, Building, Unit };
    private static readonly SpriteDrawGroup[] postLightGroups = { Particle, Unknown };

    public sealed record RenderState(Vector2i Resolution, RenderTarget FinalRenderTarget, ContentRenderers Content);

    private readonly CoreShaders shaders;
    private readonly CoreRenderSettings settings;
    private readonly Vector2Uniform gBufferResolution = new("resolution");
    private readonly FloatUniform hexagonalFallOffDistance = new("hexagonalFallOffDistance");
    private readonly IPipeline<RenderState> pipeline;

    private readonly TextureUniform heightmap = new("heightmap", TextureUnit.Texture3, null!);
    private readonly FloatUniform heightmapRadius = new("heightmapRadius");
    private readonly FloatUniform heightmapPixelSizeUV = new("heightmapPixelSizeUV");

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
            Diffuse = Texture(PixelInternalFormat.Rgba, label: "Diffuse"), // rgba
            Normal = Texture(PixelInternalFormat.Rgba, label: "Normal"), // xyz
            Depth = Texture(PixelInternalFormat.R16f, label: "Depth"), // z (0-1, camera space)
            DepthMask = DepthTexture(PixelInternalFormat.DepthComponent32f, label: "DepthMask"),

            LightAccum = Texture(PixelInternalFormat.Rgb, label: "LightAccum"),
            Composition = Texture(PixelInternalFormat.Rgba, label: "Composition"),
        };

        var targets = new
        {
            Geometry = RenderTargetWithDepthAndColors("Geometry",
                textures.DepthMask, textures.Diffuse, textures.Normal, textures.Depth),
            LightAccum = RenderTargetWithDepthAndColors("LightAccum", textures.DepthMask, textures.LightAccum),
            Composition = RenderTargetWithDepthAndColors("Composition", textures.DepthMask, textures.Composition),

            UpscaleDiffuse = RenderTargetWithColors("UpscaleDiffuse", textures.Diffuse),
            UpscaleNormal = RenderTargetWithColors("UpscaleNormal", textures.Normal),
            UpscaleDepth = RenderTargetWithColors("UpscaleDepth", textures.Depth),
            UpscaleDepthMask = RenderTargetWithDepthAndColors("UpscaleDepthMask", textures.DepthMask),
        };

        DepthBuffer = new TextureUniform("depthBuffer", TextureUnit.Texture1, textures.Depth.Texture);

        var (pointLightRenderer, spotLightRenderer) = setupLightRenderers(textures.Normal);

        var resizedBuffers = Resize(s => s.Resolution,
            textures.DepthMask, textures.Diffuse, textures.Normal,
            textures.Depth, textures.LightAccum, textures.Composition);

        var renderedGBuffers = resizedBuffers.Then(InOrder(
            WithContext(
                c => c.SetDebugName("Render level g-buffers")
                    .BindRenderTarget(targets.Geometry)
                    .SetDepthMode(Default),
                InOrder(
                    ClearColor(),
                    ClearDepth(),
                    Do(s => s.Content.LevelRenderer.Render()),
                    Do(s => worldLowResDrawGroups.ForEach(s.Content.RenderDrawGroup)),
                    WithContext(
                        c => c.SetBlendMode(Premultiplied),
                        Do(s => worldDrawGroups.ForEach(s.Content.RenderDrawGroup)))
                ))
        ));

        var compositedFrame = renderedGBuffers.Then(InOrder(
            WithContext(
                c => c.SetDebugName("Render lights to light accumulation buffer")
                    .BindRenderTarget(targets.LightAccum)
                    .SetDepthMode(TestOnly(DepthFunction.Gequal))
                    .SetBlendMode(Premultiplied)
                    .SetCullMode(RenderBack),
                InOrder(
                    ClearColor(),
                    Render(pointLightRenderer, spotLightRenderer)
                )),
            WithContext(
                c => c.SetDebugName("Composite final image")
                    .BindRenderTarget(targets.Composition),
                InOrder(
                    WithContext(
                        c => c.SetDebugName("Composite light buffer and g-buffers"),
                        PostProcess(shaders.GetShaderProgram("deferred/compose"), out _,
                            // TODO: it should be much easier to quickly pass in a couple of textures
                            new TextureUniform("albedoTexture", TextureUnit.Texture0, textures.Diffuse.Texture),
                            new TextureUniform("lightTexture", TextureUnit.Texture1, textures.LightAccum.Texture),
                            new TextureUniform("depthBuffer", TextureUnit.Texture2, textures.Depth.Texture),
                            heightmap, heightmapRadius, heightmapPixelSizeUV,
                            settings.FarPlaneBaseCorner, settings.FarPlaneUnitX, settings.FarPlaneUnitY,
                            settings.CameraPosition,
                            hexagonalFallOffDistance
                        )),
                    WithContext(
                        c => c.SetDebugName("Render fluids and other post-light groups")
                            .SetDepthMode(TestOnly(DepthFunction.Less))
                            .SetBlendMode(Premultiplied),
                        InOrder(
                            Do(s => s.Content.FluidGeometries.ForEach(f => f.Render())),
                            Do(s => postLightGroups.ForEach(s.Content.RenderDrawGroup))
                        ))
                ))
        ));

        var compositedFrameAtRenderResolution = WithContext(
            c => c.SetViewport(s => new Rectangle(0, 0, s.Resolution.X, s.Resolution.Y)),
            compositedFrame
        );

        var fullRender = compositedFrameAtRenderResolution.Then(
            WithContext(
                c => c.SetDebugName("Copy/scale image to output render target")
                    .BindRenderTarget(s => s.FinalRenderTarget),
                PostProcess(shaders.GetShaderProgram("deferred/copy"), out _,
                    new TextureUniform("inputTexture", TextureUnit.Texture0, textures.Composition.Texture),
                    new Vector2Uniform("uvOffset", Vector2.Zero)
                )
            )
        );

        pipeline = WithContext(
            c => c.SetDebugName("Render game state"),
            fullRender);

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
        var resolution = calculateResolution(deferredLayer.CameraDistance, Constants.Rendering.PixelsPerTileCompositeResolution);
        gBufferResolution.Value = new Vector2(resolution.X, resolution.Y);
        hexagonalFallOffDistance.Value = deferredLayer.HexagonalFallOffDistance;

        prepareLevel(deferredLayer.ContentRenderers.LevelRenderer);

        var state = new RenderState(resolution, target, deferredLayer.ContentRenderers);

        pipeline.Execute(state);

        // cleaning of mesh builders will happen in game renderer
    }

    private void prepareLevel(LevelRenderer renderer)
    {
        renderer.PrepareForRender();
        var hmap = renderer.Heightmap;
        heightmap.Value = hmap.Texture;
        heightmapRadius.Value = hmap.RadiusUniform.Value;
        heightmapPixelSizeUV.Value = hmap.PixelSizeUVUniform.Value;
    }

    public void OnResize(ViewportSize newViewport)
    {
        viewport = newViewport;
    }

    private Vector2i calculateResolution(float cameraDistance, float pixelsPerTile)
    {
        var screenPixelsPerTile = viewport.Height * 0.5f / cameraDistance;

        var scale = Math.Min(1, pixelsPerTile / screenPixelsPerTile);

        var bufferSizeFactor = UserSettings.Instance.Graphics.SuperSample;

        var w = (int)(viewport.Width * bufferSizeFactor * scale);
        var h = (int)(viewport.Height * bufferSizeFactor * scale);

        return (w, h);
    }

    public void ClearAll()
    {
        PointLights.Clear();
        Spotlights.Clear();
    }
}
