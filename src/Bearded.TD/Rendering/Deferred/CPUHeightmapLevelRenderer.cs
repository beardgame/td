using System;
using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.World;
using Bearded.TD.Meta;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static Bearded.TD.Constants.Game.World;
using static Bearded.TD.Tiles.Direction;

namespace Bearded.TD.Rendering.Deferred
{
    class CPUHeightmapLevelRenderer : LevelRenderer
    {
        private const float heightMapScale = 4;
        private const float gridScale = 6;

        private const float worldToGrid = gridScale;
        private const float gridToWorld = 1 / gridScale;

        private static readonly (Vector2, Step)[] gridNeighbourOffsets =
            new[] {DownRight, Right, UpRight, UpLeft, Left, DownLeft}
                .Select(d => (d.Vector() * gridToWorld, d.Step()))
                .ToArray();

        private readonly HashSet<Tile> dirtyTiles = new HashSet<Tile>();

        private readonly Level level;
        private readonly GeometryLayer geometryLayer;

        private readonly float fallOffDistance;

        private readonly ExpandingVertexSurface<LevelVertex> heightMapSurface;
        private readonly float heightMapWorldSize;
        private readonly int heightMapResolution;
        private readonly float[,] heightMap;

        private readonly int gridRadius;
        private readonly Tilemap<float> sampledHeight;
        private readonly Tilemap<Vector3> normals;

        private bool regenerateEntireHeightMap = true;

        public CPUHeightmapLevelRenderer(GameInstance game, RenderContext context, Material material)
            : base(game)
        {
            level = game.State.Level;
            geometryLayer = game.State.GeometryLayer;

            var tileMapWidth = level.Radius * 2 + 1;
            var gridWidth = tileMapWidth * gridScale;
            gridRadius = (int) (gridWidth - 1) / 2;

            fallOffDistance = (level.Radius - 0.25f) * HexagonWidth;

            heightMapWorldSize = tileMapWidth * HexagonWidth;
            heightMapResolution = (int) (tileMapWidth * heightMapScale);
            heightMap = new float[heightMapResolution, heightMapResolution];

            sampledHeight = new Tilemap<float>(gridRadius);
            normals = new Tilemap<Vector3>(gridRadius);

            var surface = new ExpandingVertexSurface<LevelVertex>()
                {
                    ClearOnRender = false,
                    IsStatic = true,
                }
                .WithShader(material.Shader.SurfaceShader)
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

            heightMapSurface = surface;
        }

        protected override void OnTileChanged(Tile tile)
        {
            dirtyTiles.Add(tile);
        }

        public override void RenderAll()
        {
            if (regenerateEntireHeightMap)
                regenerateHeightMap();

            if (dirtyTiles.Count > 0)
                regenerateDirtyTiles();

            renderHeightMap();
        }

        private void regenerateDirtyTiles()
        {
            var dirtyGridTiles = new HashSet<Tile>();

            foreach (var tile in dirtyTiles)
            {
                var gridCenter = Level.GetTile(new Position2(Level.GetPosition(tile).NumericValue * worldToGrid));

                foreach (var gridTile in Tilemap
                    .GetSpiralCenteredAt(gridCenter, (int) gridScale)
                    .Where(t => t.Radius < gridRadius))
                {
                    dirtyGridTiles.Add(gridTile);
                }
            }

            dirtyTiles.Clear();

            regenerate(dirtyGridTiles);
        }

        private void regenerate(HashSet<Tile> dirtyGridTiles)
        {
            // TODO: don't sample entire world
            sampleWorldToHeightmap();

            sampleHeightmapToHeights(dirtyGridTiles);

            calculateNormals(dirtyGridTiles);

            // TODO: batch height map? (could draw entire map as one list of triangles actually, but batching helps performance when zoomed in)
            populateHeightMapSurface();
        }

        private void renderHeightMap()
        {
            if (UserSettings.Instance.Debug.WireframeLevel)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                heightMapSurface.Render();
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
            else
            {
                heightMapSurface.Render();
            }
        }


        private void regenerateHeightMap()
        {
            sampleWorldToHeightmap();

            sampleHeightmapToHeights(Tilemap.EnumerateTilemapWith(gridRadius));

            calculateNormals(Tilemap.EnumerateTilemapWith(gridRadius - 1));

            populateHeightMapSurface();

            regenerateEntireHeightMap = false;
            dirtyTiles.Clear();
        }

        private void sampleWorldToHeightmap()
        {
            foreach (var y in Enumerable.Range(0, heightMapResolution))
            {
                foreach (var x in Enumerable.Range(0, heightMapResolution))
                {
                    var position = positionOfHeightmapPixel(x, y);
                    var tile = Level.GetTile(position);

                    if (!level.IsValid(tile))
                    {
                        heightMap[x, y] = Single.NaN;
                        continue;
                    }

                    var tileGeometry = geometryLayer[tile];

                    var height = tileGeometry.DrawInfo.Height;

                    heightMap[x, y] = height.NumericValue;
                }
            }
        }

        private void sampleHeightmapToHeights(IEnumerable<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                var p = Level.GetPosition(tile).NumericValue * gridToWorld;

                var h = heightMapValueAt(p);

                sampledHeight[tile] = h;
            }
        }

        private void calculateNormals(IEnumerable<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                var height = sampledHeight[tile];

                var (vectorPrev, stepPrev) = gridNeighbourOffsets.Last();
                var heightPrevOffset = sampledHeight[tile.Offset(stepPrev)] - height;

                var normalAccumulator = Vector3.Zero;

                foreach (var (vectorCurrent, stepCurrent) in gridNeighbourOffsets)
                {
                    var heightCurrentOffset = sampledHeight[tile.Offset(stepCurrent)] - height;

                    var triangleNormal = Vector3.Cross(
                        vectorPrev.WithZ(heightPrevOffset),
                        vectorCurrent.WithZ(heightCurrentOffset)
                    );

                    normalAccumulator += triangleNormal;

                    vectorPrev = vectorCurrent;
                    heightPrevOffset = heightCurrentOffset;
                }

                normals[tile] = normalAccumulator.NormalizedSafe();
            }
        }

        private void populateHeightMapSurface()
        {
            heightMapSurface.Clear();

            /* Vertex layout
             * -- v3
             *   /  \
             * v0 -- v2
             *   \  /
             * -- v1
             */

            var (v1Offset, v1Step) = gridNeighbourOffsets[0];
            var (v2Offset, v2Step) = gridNeighbourOffsets[1];
            var (v3Offset, v3Step) = gridNeighbourOffsets[2];

            foreach (var tile in Tilemap.EnumerateTilemapWith(gridRadius - 1))
            {
                var t1 = tile.Offset(v1Step);
                var t2 = tile.Offset(v2Step);
                var t3 = tile.Offset(v3Step);

                var v0 = Level.GetPosition(tile).NumericValue * gridToWorld;
                var v1 = v0 + v1Offset;
                var v2 = v0 + v2Offset;
                var v3 = v0 + v3Offset;

                var h0 = sampledHeight[tile];
                var h1 = sampledHeight[t1];
                var h2 = sampledHeight[t2];
                var h3 = sampledHeight[t3];

                var n0 = normals[tile];
                var n1 = normals[t1];
                var n2 = normals[t2];
                var n3 = normals[t3];

                if (Single.IsNaN(h0) || Single.IsNaN(h1) || Single.IsNaN(h2))
                {
                    /*
                    heightMapSurface.AddVertices(
                        new LevelVertex(v0.WithZ(), n0, Vector2.Zero, Color.Red),
                        new LevelVertex(v2.WithZ(), n2, Vector2.Zero, Color.Red),
                        new LevelVertex(v1.WithZ(), n1, Vector2.Zero, Color.Red)
                    );
                    */
                }
                else
                {
                    heightMapSurface.AddVertices(
                        vertex(v0.WithZ(h0), n0, Vector2.Zero, Color.White),
                        vertex(v2.WithZ(h2), n2, Vector2.Zero, Color.White),
                        vertex(v1.WithZ(h1), n1, Vector2.Zero, Color.White)
                    );
                }

                if (Single.IsNaN(h0) || Single.IsNaN(h3) || Single.IsNaN(h2))
                {
                    /*
                    heightMapSurface.AddVertices(
                        new LevelVertex(v0.WithZ(), n0, Vector2.Zero, Color.Red),
                        new LevelVertex(v3.WithZ(), n3, Vector2.Zero, Color.Red),
                        new LevelVertex(v2.WithZ(), n2, Vector2.Zero, Color.Red)
                    );
                    */
                }
                else
                {
                    heightMapSurface.AddVertices(
                        vertex(v0.WithZ(h0), n0, Vector2.Zero, Color.White),
                        vertex(v3.WithZ(h3), n3, Vector2.Zero, Color.White),
                        vertex(v2.WithZ(h2), n2, Vector2.Zero, Color.White)
                    );
                }
            }
        }

        private LevelVertex vertex(Vector3 v, Vector3 n, Vector2 uv, Color c)
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

        private Position2 positionOfHeightmapPixel(int x, int y)
        {
            return new Position2
            (
                (x / (float) heightMapResolution - 0.5f) * heightMapWorldSize,
                (y / (float) heightMapResolution - 0.5f) * heightMapWorldSize
            );
        }

        private float heightMapValueAt(Vector2 point)
        {
            var pointInMap = (point / heightMapWorldSize + new Vector2(0.5f)) * heightMapResolution;

            var x0 = Math.Min((int) pointInMap.X, heightMapResolution - 2);
            var y0 = Math.Min((int) pointInMap.Y, heightMapResolution - 2);

            var xt = pointInMap.X - x0;
            var yt = pointInMap.Y - y0;

            var height = Interpolate.Lerp(
                Interpolate.Lerp(heightMap[x0, y0], heightMap[x0 + 1, y0], xt),
                Interpolate.Lerp(heightMap[x0, y0 + 1], heightMap[x0 + 1, y0 + 1], xt),
                yt
            );

            return height;
        }

        public override void CleanUp()
        {
            heightMapSurface.Dispose();
        }
    }
}
