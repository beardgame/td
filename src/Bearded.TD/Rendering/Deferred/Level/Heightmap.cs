using System;
using System.Drawing;
using Bearded.Graphics.Pipelines;
using Bearded.Graphics.RenderSettings;
using Bearded.TD.Game;
using Bearded.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Deferred.Level
{
    sealed class Heightmap : IDisposable
    {
        private readonly int tileMapWidth;

        private readonly PipelineTexture texture;
        private readonly PipelineRenderTarget renderTarget; // H
        public FloatUniform RadiusUniform { get; } = new("heightmapRadius");
        public FloatUniform PixelSizeUVUniform { get; } = new("heightmapPixelSizeUV");

        private float pixelsPerTile;
        private int resolution;

        public event VoidEventHandler? ResolutionChanged;

        public Heightmap(GameInstance game)
        {
            tileMapWidth = game.State.Level.Radius * 2 + 1;
            RadiusUniform.Value = tileMapWidth * Constants.Game.World.HexagonWidth * 0.5f;

            texture = Pipeline.Texture(PixelInternalFormat.R16f, setup: t =>
            {
                t.SetFilterMode(TextureMinFilter.Linear, TextureMagFilter.Linear);
                t.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            });

            renderTarget = Pipeline.RenderTargetWithColors(texture);
        }

        public IPipeline<T> DrawHeights<T>(IPipeline<T> render)
        {
            return Pipeline<T>.WithContext(
                c => c.BindRenderTarget(renderTarget)
                    .SetViewport(_ => new Rectangle(0, 0, resolution, resolution)),
                render
            );
        }

        public TextureUniform GetHeightmapUniform(string name, TextureUnit unit)
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
            texture.EnsureSize(new Vector2i(resolution, resolution));
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
}
