using System;
using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Meta;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static System.Single;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Rendering.Deferred
{
    class CPUHeightmapLevelRenderer : HeightmapLevelRenderer
    {
        private readonly HashSet<Tile> dirtyTiles = new HashSet<Tile>();

        private readonly ExpandingVertexSurface<LevelVertex> heightMapSurface;
        private readonly float[,] heightMap;

        private readonly Tilemap<float> sampledHeight;
        private readonly Tilemap<Vector3> normals;

        private bool regenerateEntireHeightMap = true;

        public CPUHeightmapLevelRenderer(GameInstance game, RenderContext context, Material material)
            : base(game, context, material)
        {
            heightMap = new float[HeightMapResolution, HeightMapResolution];

            sampledHeight = new Tilemap<float>(GridRadius);
            normals = new Tilemap<Vector3>(GridRadius);

            heightMapSurface = new ExpandingVertexSurface<LevelVertex>
            {
                ClearOnRender = false,
                IsStatic = true,
            };

            UseMaterialOnSurface(heightMapSurface);
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
                var gridCenter = Level.GetTile(new Position2(Level.GetPosition(tile).NumericValue * WorldToGrid));

                foreach (var gridTile in Tilemap
                    .GetSpiralCenteredAt(gridCenter, (int) GridScale)
                    .Where(t => t.Radius < GridRadius))
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

            sampleHeightmapToHeights(Tilemap.EnumerateTilemapWith(GridRadius));

            calculateNormals(Tilemap.EnumerateTilemapWith(GridRadius - 1));

            populateHeightMapSurface();

            regenerateEntireHeightMap = false;
            dirtyTiles.Clear();
        }

        private void sampleWorldToHeightmap()
        {
            foreach (var y in Enumerable.Range(0, HeightMapResolution))
            {
                foreach (var x in Enumerable.Range(0, HeightMapResolution))
                {
                    var position = positionOfHeightmapPixel(x, y);
                    var tile = Level.GetTile(position);

                    if (!Level.IsValid(tile))
                    {
                        heightMap[x, y] = NaN;
                        continue;
                    }

                    var tileGeometry = GeometryLayer[tile];

                    var height = tileGeometry.DrawInfo.Height;

                    heightMap[x, y] = height.NumericValue;
                }
            }
        }

        private void sampleHeightmapToHeights(IEnumerable<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                var p = Level.GetPosition(tile).NumericValue * GridToWorld;

                var h = heightMapValueAt(p);

                sampledHeight[tile] = h;
            }
        }

        private void calculateNormals(IEnumerable<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                var height = sampledHeight[tile];

                var (vectorPrev, stepPrev) = GridNeighbourOffsets.Last();
                var heightPrevOffset = sampledHeight[tile.Offset(stepPrev)] - height;

                var normalAccumulator = Vector3.Zero;

                foreach (var (vectorCurrent, stepCurrent) in GridNeighbourOffsets)
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

            GenerateGrid(
                (t0, t1, t2, t3, v0, v1, v2, v3) =>
                {
                    var h0 = sampledHeight[t0];
                    var h1 = sampledHeight[t1];
                    var h2 = sampledHeight[t2];
                    var h3 = sampledHeight[t3];

                    var n0 = normals[t0];
                    var n1 = normals[t1];
                    var n2 = normals[t2];
                    var n3 = normals[t3];

                    if (IsNaN(h0) || IsNaN(h1) || IsNaN(h2))
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
                            Vertex(v0.WithZ(h0), n0, Vector2.Zero, Color.White),
                            Vertex(v2.WithZ(h2), n2, Vector2.Zero, Color.White),
                            Vertex(v1.WithZ(h1), n1, Vector2.Zero, Color.White)
                        );
                    }

                    if (IsNaN(h0) || IsNaN(h3) || IsNaN(h2))
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
                            Vertex(v0.WithZ(h0), n0, Vector2.Zero, Color.White),
                            Vertex(v3.WithZ(h3), n3, Vector2.Zero, Color.White),
                            Vertex(v2.WithZ(h2), n2, Vector2.Zero, Color.White)
                        );
                    }
                }
                );
        }


        private Position2 positionOfHeightmapPixel(int x, int y)
        {
            return new Position2
            (
                (x / (float) HeightMapResolution - 0.5f) * HeightMapWorldSize,
                (y / (float) HeightMapResolution - 0.5f) * HeightMapWorldSize
            );
        }

        private float heightMapValueAt(Vector2 point)
        {
            var pointInMap = (point / HeightMapWorldSize + new Vector2(0.5f)) * HeightMapResolution;

            var x0 = Math.Min((int) pointInMap.X, HeightMapResolution - 2);
            var y0 = Math.Min((int) pointInMap.Y, HeightMapResolution - 2);

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
