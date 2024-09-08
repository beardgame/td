using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Pipelines;
using Bearded.Graphics.Pipelines.Context;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Textures;
using Bearded.TD.Content.Models;
using Bearded.TD.Meta;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using static OpenTK.Graphics.OpenGL.BlendEquationMode;
using static OpenTK.Graphics.OpenGL.BlendingFactor;
using static OpenTK.Graphics.OpenGL.DepthFunction;
using Color = Bearded.Graphics.Color;

namespace Bearded.TD.Rendering;

using static BlendMode;
using static CullMode;
using static DepthMode;
using static DrawOrderGroup;
using static Pipeline;
using static Pipeline<DeferredRenderer.RenderState>;

sealed class DeferredRenderer
{
    private static readonly BlendMode invertedPremultiplied = new(OneMinusDstAlpha, One, FuncAdd);

    private static readonly DrawOrderGroup[] solidLevelDrawGroups = [Level, SolidLevelDetails];
    private static readonly DrawOrderGroup[] levelDecalDrawGroup = [LevelProjected];
    private static readonly DrawOrderGroup[] projectedFloatingDrawGroup = [LevelProjectedFloatingOverlay];
    private static readonly DrawOrderGroup[] worldDrawGroups = [Building, Unit];
    private static readonly DrawOrderGroup[] worldDetailGroups = [LevelDetail];
    private static readonly DrawOrderGroup[] postLightGroups = [Fluids, Particle, Unknown];
    private static readonly DrawOrderGroup[] ignoreDepthGroup = [IgnoreDepth];

    public sealed record RenderState(Vector2i Resolution, RenderTarget FinalRenderTarget, DeferredContent Content);

    private readonly CoreShaders shaders;
    private readonly CoreRenderSettings settings;
    private readonly Vector2Uniform gBufferResolution = new("resolution");
    private readonly FloatUniform hexagonalFallOffDistance = new("hexagonalFallOffDistance");
    private readonly IPipeline<RenderState> pipeline;

    private readonly TextureUniform heightmapTexture = new("heightmap", TextureUnit.Texture3, null!);
    private readonly FloatUniform heightmapRadius = new("heightmapRadius");
    private readonly FloatUniform heightmapPixelSizeUV = new("heightmapPixelSizeUV");

    private ViewportSize viewport;

    private readonly Texture depthBufferTexture;

    public IRenderSetting GetDepthBufferUniform(string name, TextureUnit unit)
        => new TextureUniform(name, unit, depthBufferTexture);

    public readonly PointLightMesh PointLights = PointLightMesh.Create();

    public ExpandingIndexedTrianglesMeshBuilder<SpotlightVertex> Spotlights { get; } = new();

    public DeferredRenderer(
        CoreRenderSettings settings,
        CoreShaders coreShaders)
    {
        this.settings = settings;
        shaders = coreShaders;

        var textures = new
        {
            Diffuse = Texture(PixelInternalFormat.Rgba, label: "Diffuse"), // rgba
            Normal = Texture(PixelInternalFormat.Rgba, label: "Normal"), // xyz
            Material = Texture(PixelInternalFormat.Rg8, label: "Material"), // roughness, metallic, AO
            Depth = Texture(PixelInternalFormat.R16f, label: "Depth"), // z (0-1, camera space)
            DepthMask = DepthTexture(PixelInternalFormat.DepthComponent32f, label: "DepthMask"),

            LightAccum = Texture(PixelInternalFormat.Rgb, label: "LightAccum"),
            Composition = Texture(PixelInternalFormat.Rgba, label: "Composition"),
        };

        var targets = new
        {
            Geometry = RenderTargetWithDepthAndColors("Geometry",
                textures.DepthMask, textures.Diffuse, textures.Normal, textures.Depth),
            GeometryDiffuseOnly = RenderTargetWithDepthAndColors("GeometryDiffuseOnly",
                textures.DepthMask, textures.Diffuse),

            LightAccum = RenderTargetWithDepthAndColors("LightAccum", textures.DepthMask, textures.LightAccum),
            Composition = RenderTargetWithDepthAndColors("Composition", textures.DepthMask, textures.Composition),

            UpscaleDiffuse = RenderTargetWithColors("UpscaleDiffuse", textures.Diffuse),
            UpscaleNormal = RenderTargetWithColors("UpscaleNormal", textures.Normal),
            UpscaleDepth = RenderTargetWithColors("UpscaleDepth", textures.Depth),
            UpscaleDepthMask = RenderTargetWithDepthAndColors("UpscaleDepthMask", textures.DepthMask),
        };

        depthBufferTexture = textures.Depth.Texture;

        var (pointLightRenderer, spotLightRenderer) = setupLightRenderers(
            textures.Normal, textures.Material, textures.Diffuse);

        var resizedBuffers = Resize(s => s.Resolution,
            textures.DepthMask, textures.Diffuse, textures.Normal, textures.Material,
            textures.Depth, textures.LightAccum, textures.Composition);

        var renderedGBuffers = resizedBuffers.Then(InOrder(
            WithContext(
                c => c.SetDebugName("Render level g-buffers")
                    .BindRenderTarget(targets.Geometry)
                    .SetDepthMode(Default),
                InOrder(
                    ClearColor(),
                    ClearDepth(),
                    renderDrawGroups(solidLevelDrawGroups),
                    WithContext(
                        c => c.SetDebugName("Render level decals")
                            .BindRenderTarget(targets.GeometryDiffuseOnly)
                            .SetDepthMode(TestOnly(Gequal))
                            .SetBlendMode(Premultiplied),
                        renderDrawGroups(levelDecalDrawGroup)
                    ),
                    WithContext(
                        c => c.SetDebugName("Render projected floating overlays")
                            .BindRenderTarget(targets.Composition)
                            .SetDepthMode(DepthMode.Disable)
                            .SetBlendMode(Premultiplied)
                            .SetCullMode(RenderFront),
                        InOrder(
                            ClearColor(Color.Transparent),
                            renderDrawGroups(projectedFloatingDrawGroup)
                        )
                    ),
                    WithContext(
                        c => c.SetBlendMode(Premultiplied),
                        renderDrawGroups(worldDrawGroups))
                )),
            WithContext(
                c => c.SetDebugName("Render level details")
                    .BindRenderTarget(targets.GeometryDiffuseOnly)
                    .SetDepthMode(TestOnly(Less))
                    .SetBlendMode(Premultiplied),
                renderDrawGroups(worldDetailGroups)
                )
        ));

        var compositedFrame = renderedGBuffers.Then(InOrder(
            WithContext(
                c => c.SetDebugName("Render lights to light accumulation buffer")
                    .BindRenderTarget(targets.LightAccum)
                    .SetDepthMode(TestOnly(Gequal))
                    .SetBlendMode(Premultiplied)
                    .SetCullMode(RenderBack),
                InOrder(
                    ClearColor(),
                    Render(pointLightRenderer, spotLightRenderer)
                )),
            WithContext(
                c => c.SetDebugName("Composite final image")
                    .BindRenderTarget(targets.Composition)
                    .SetBlendMode(Premultiplied),
                InOrder(
                    WithContext(
                        c => c.SetDebugName("Composite light buffer and g-buffers")
                            .SetBlendMode(invertedPremultiplied),
                        PostProcess(shaders.GetShaderProgram("deferred/compose"), out _,
                            TextureUniforms(
                                (textures.Diffuse, "albedoTexture"),
                                (textures.LightAccum, "lightTexture"),
                                (textures.Depth, "depthBuffer")
                                ),
                            heightmapTexture, heightmapRadius, heightmapPixelSizeUV,
                            settings.FarPlaneBaseCorner, settings.FarPlaneUnitX, settings.FarPlaneUnitY,
                            settings.CameraPosition,
                            hexagonalFallOffDistance
                        )),
                    WithContext(
                        c => c.SetDebugName("Render fluids and other post-light groups")
                            .SetDepthMode(TestOnly(Less)),
                        renderDrawGroups(postLightGroups)
                        ),
                    WithContext(
                        c => c.SetDebugName("Render depth ignoring groups"),
                        renderDrawGroups(ignoreDepthGroup)
                        )
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
                    new TextureUniform("inputTexture", TextureUnit.Texture0, textures.Composition.Texture)
                )
            )
        );

        pipeline = WithContext(
            c => c.SetDebugName("Render game state"),
            fullRender);
    }

    private static IPipeline<RenderState> renderDrawGroups(IEnumerable<DrawOrderGroup> drawGroups)
    {
        return InOrder(drawGroups.Select(group =>
            WithContext(c => c.SetDebugName($"Group {group}"), Do(s => s.Content.RenderDrawGroup(group)))
        ));
    }

    private (IRenderer pointLightRenderer, IRenderer spotLightRenderer) setupLightRenderers(
        PipelineTexture normalBuffer, PipelineTexture materialBuffer, PipelineTexture diffuse)
    {
        var neededSettings = new[]
        {
            settings.ViewMatrix, settings.ProjectionMatrix,
            settings.FarPlaneBaseCorner, settings.FarPlaneUnitX, settings.FarPlaneUnitY, settings.CameraPosition,
            new TextureUniform("normalBuffer", TextureUnit.Texture0, normalBuffer.Texture),
            GetDepthBufferUniform("depthBuffer", TextureUnit.Texture1), gBufferResolution,
            new TextureUniform("materialBuffer", TextureUnit.Texture2, materialBuffer.Texture),
            new TextureUniform("diffuseBuffer", TextureUnit.Texture3, diffuse.Texture),
        };

        var pointLight = Renderer.From(PointLights.ToRenderable(), neededSettings);
        shaders.GetShaderProgram("deferred/pointlight").UseOnRenderer(pointLight);

        var spotLight = BatchedRenderer.From(Spotlights.ToRenderable(), neededSettings);
        shaders.GetShaderProgram("deferred/spotlight").UseOnRenderer(spotLight);

        return (pointLight, spotLight);
    }

    public void RenderLayer(IDeferredRenderLayer deferredLayer, RenderTarget target)
    {
        var resolution = calculateResolution();
        prepareForRendering(deferredLayer, resolution);
        render(deferredLayer, target, resolution);
    }

    private void render(IDeferredRenderLayer deferredLayer, RenderTarget target, Vector2i resolution)
    {
        var state = new RenderState(resolution, target, deferredLayer.Content);

        pipeline.Execute(state);
    }

    private void prepareForRendering(IDeferredRenderLayer deferredLayer, Vector2i resolution)
    {
        prepareUniforms(deferredLayer, resolution);
    }

    private void prepareUniforms(IDeferredRenderLayer deferredLayer, Vector2i resolution)
    {
        var heightmap = deferredLayer.Content.Heightmap;
        heightmapTexture.Value = heightmap.Texture;
        heightmapRadius.Value = heightmap.Radius;
        heightmapPixelSizeUV.Value = heightmap.PixelSizeUV;
        gBufferResolution.Value = new Vector2(resolution.X, resolution.Y);
        hexagonalFallOffDistance.Value = deferredLayer.HexagonalFallOffDistance;
    }

    public void OnResize(ViewportSize newViewport)
    {
        viewport = newViewport;
    }

    private Vector2i calculateResolution()
    {
        var bufferSizeFactor = UserSettings.Instance.Graphics.SuperSample;

        var w = (int)(viewport.Width * bufferSizeFactor);
        var h = (int)(viewport.Height * bufferSizeFactor);

        return (w, h);
    }

    public void ClearAll()
    {
        PointLights.Clear();
        Spotlights.Clear();
    }
}
