using System;
using System.Drawing;
using Bearded.Graphics.Pipelines;
using Bearded.Graphics.Pipelines.Context;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Textures;
using Bearded.TD.Game;
using Bearded.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Deferred.Level;

abstract class LevelTextureMap : IDisposable
{
    private readonly int tileMapWidth;

    private readonly PipelineTexture texture; // H, V
    private readonly PipelineRenderTarget renderTarget;
    private readonly IPipeline<Vector2i> resizeTexture;

    public FloatUniform RadiusUniform { get; }
    public FloatUniform PixelSizeUVUniform { get; }
    public Texture Texture => texture.Texture;

    private float pixelsPerTile;
    private int resolution;

    public event VoidEventHandler? ResolutionChanged;

    public LevelTextureMap(GameInstance game, string uniformPrefix, PixelInternalFormat pixelFormat)
    {
        RadiusUniform = new FloatUniform(uniformPrefix + "Radius");
        PixelSizeUVUniform = new FloatUniform(uniformPrefix + "PixelSizeUV");

        tileMapWidth = game.State.Level.Radius * 2 + 1;
        RadiusUniform.Value = tileMapWidth * Constants.Game.World.HexagonWidth * 0.5f;

        texture = Pipeline.Texture(pixelFormat, setup: t =>
        {
            t.SetFilterMode(TextureMinFilter.Linear, TextureMagFilter.Linear);
            t.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
        });

        renderTarget = Pipeline.RenderTargetWithColors(texture);

        resizeTexture = Pipeline<Vector2i>.InOrder(
            Pipeline<Vector2i>.Resize(s => s, texture),
            DrawWithMask(Pipeline<Vector2i>.ClearColor(0, 0, 0, 0), ColorMask.DrawAll)
        );
    }

    protected IPipeline<T> DrawWithMask<T>(IPipeline<T> render, ColorMask mask)
    {
        return Pipeline<T>.WithContext(
            c => c.BindRenderTarget(renderTarget)
                .SetViewport(_ => new Rectangle(0, 0, resolution, resolution))
                .SetColorMask(mask),
            render
        );
    }

    public TextureUniform GetMapTextureUniform(string name, TextureUnit unit)
    {
        return new(name, unit, texture.Texture);
    }

    public void SetScale(float scale)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (pixelsPerTile == scale)
            return;

        pixelsPerTile = scale;
        recalculateResolution();
        resizeTexture.Execute(new Vector2i(resolution, resolution));

        ResolutionChanged?.Invoke();
    }

    private void recalculateResolution()
    {
        resolution = (int)(tileMapWidth * pixelsPerTile);
        PixelSizeUVUniform.Value = 1f / resolution;
    }

    public void Dispose()
    {
        renderTarget.Dispose();
        texture.Dispose();
    }
}
