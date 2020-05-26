using System;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static Bearded.TD.Constants.Game.World;
using static Bearded.TD.Tiles.Direction;

namespace Bearded.TD.Rendering.Deferred
{
    abstract class HeightmapLevelRenderer : LevelRenderer
    {
        protected delegate void GenerateTile(
            Tile t0, Tile t1, Tile t2, Tile t3,
            Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3
            );

        protected static readonly (Vector2, Step)[] GridNeighbourOffsets =
            new[] {DownRight, Right, UpRight, UpLeft, Left, DownLeft}
                .Select(d => (d.Vector() * GridToWorld, d.Step()))
                .ToArray();

        protected const float HeightMapScale = 50;
        protected const float GridScale = 10;
        protected const float WorldToGrid = GridScale;
        protected const float GridToWorld = 1 / GridScale;

        private readonly RenderContext context;
        private readonly Material material;

        protected readonly Level Level;
        protected readonly GeometryLayer GeometryLayer;
        protected readonly float HeightMapWorldSize;
        protected readonly int HeightMapResolution;
        protected readonly int GridRadius;

        private readonly float fallOffDistance;

        protected HeightmapLevelRenderer(GameInstance game, RenderContext context, Material material)
            : base(game)
        {
            this.context = context;
            this.material = material;
            Level = game.State.Level;
            GeometryLayer = game.State.GeometryLayer;

            var tileMapWidth = Level.Radius * 2 + 1;
            var gridWidth = tileMapWidth * GridScale;
            GridRadius = (int) (gridWidth - 1) / 2;

            HeightMapWorldSize = tileMapWidth * HexagonWidth;
            HeightMapResolution = (int) (tileMapWidth * HeightMapScale);

            fallOffDistance = (Level.Radius - 0.25f) * HexagonWidth;
        }

        protected void UseMaterialOnSurface(Surface surface)
        {
            surface.WithShader(material.Shader.SurfaceShader)
                .AndSettings(
                    context.Surfaces.ViewMatrixLevel,
                    context.Surfaces.ProjectionMatrix,
                    context.Surfaces.FarPlaneDistance
                );

            var textureUnit = TextureUnit.Texture0;

            foreach (var texture in material.ArrayTextures)
            {
                surface.AddSetting(new ArrayTextureUniform(texture.UniformName, texture.Texture, textureUnit));

                textureUnit++;
            }
        }

        protected void GenerateGrid(GenerateTile generateTile)
        {
            /* Vertex layout
             * -- v3
             *   /  \
             * v0 -- v2
             *   \  /
             * -- v1
             */

            var (v1Offset, v1Step) = GridNeighbourOffsets[0];
            var (v2Offset, v2Step) = GridNeighbourOffsets[1];
            var (v3Offset, v3Step) = GridNeighbourOffsets[2];

            foreach (var t0 in Tilemap.EnumerateTilemapWith(GridRadius - 1))
            {
                var t1 = t0.Offset(v1Step);
                var t2 = t0.Offset(v2Step);
                var t3 = t0.Offset(v3Step);

                var v0 = Level.GetPosition(t0).NumericValue * GridToWorld;
                var v1 = v0 + v1Offset;
                var v2 = v0 + v2Offset;
                var v3 = v0 + v3Offset;

                generateTile(t0, t1, t2, t3, v0, v1, v2, v3);
            }
        }

        protected LevelVertex Vertex(Vector3 v, Vector3 n, Vector2 uv, Color c)
        {
            var a = 1f; //(1 - Abs(v.Z * v.Z * 1f)).Clamped(0f, 1);

            var distanceFalloff = ((fallOffDistance - hexagonalDistanceToOrigin(v.Xy)) * 0.3f)
                .Clamped(0f, 1f).Squared();

            a *= distanceFalloff;

            return new LevelVertex(v, n, uv, new Color(c * a, c.A));
        }

        private static float hexagonalDistanceToOrigin(Vector2 xy)
        {
            var yf = xy.Y * (1 / HexagonDistanceY);
            var xf = xy.X * (1 / HexagonWidth) - yf * 0.5f;
            var x = Math.Abs(xf);
            var y = Math.Abs(yf);
            var reduction = Math.Sign(xf) != Math.Sign(yf) ? Math.Min(x, y) : 0f;
            return x + y - reduction;
        }
    }
}
