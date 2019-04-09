using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.Events;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering.Deferred
{
    class LevelGeometryManager : IListener<TileDrawInfoChanged>
    {
        private const int batchWidth = 8;

        private readonly int tileMapRadius;
        private readonly int batchesPerRow;
        private readonly Batch[] batchLookup;
        private readonly List<Batch> batches = new List<Batch>();

        private readonly Level level;
        private readonly GeometryLayer geometryLayer;

        // TODO: in the future this also needs to know about textures and stuff from mods
        public LevelGeometryManager(GameInstance game, RenderContext context, Material material)
        {
            level = game.State.Level;
            geometryLayer = game.State.GeometryLayer;
            tileMapRadius = level.Radius;

            var tileMapWidth = tileMapRadius * 2 + 1;

            batchesPerRow = (tileMapWidth + batchWidth - 1) / batchWidth;

            batchLookup = new Batch[batchesPerRow * batchesPerRow];

            createValidBatches(context, material);

            game.Meta.Events.Subscribe(this);
        }

        public void HandleEvent(TileDrawInfoChanged @event)
        {
            var tile = @event.Tile;

            var (x, y) = (tile.X, tile.Y);

            markDirty((x, y));
            markDirty((x - 1, y));
            markDirty((x, y - 1));
            markDirty((x - 1, y + 1));

        }

        private void markDirty((int X, int Y) tile)
        {
            var batchIndex = batchIndexFor(tile);
            batchLookup[batchIndex].MarkAsDirty();
        }

        public void RenderAll()
        {
            foreach (var batch in batches)
            {
                batch.Render();
            }
        }

        private void createValidBatches(RenderContext context, Material material)
        {
            foreach (var i in Enumerable
                .Range(0, batchLookup.Length)
                .Where(isValidBatch))
            {
                var baseTile = baseTileFor(i);
                var batch = new Batch(context, material, level, geometryLayer, baseTile);

                batchLookup[i] = batch;
                batches.Add(batch);
            }
        }

        private bool isValidBatch(int i)
        {
            var baseTile = baseTileFor(i);

            for (var x = 0; x < batchWidth; x++)
                for (var y = 0; y < batchWidth; y++)
                {
                    if (level.IsValid(new Tile(baseTile.X + x, baseTile.Y + y)))
                        return true;
                }

            return false;
        }

        private (int X, int Y) baseTileFor(int batchIndex)
            => (
                X: (batchIndex % batchesPerRow) * batchWidth - tileMapRadius,
                Y: (batchIndex / batchesPerRow) * batchWidth - tileMapRadius
            );

        private int batchIndexFor((int x, int y) tile)
            => batchIndexFor(tile.x, tile.y);

        private int batchIndexFor(int x, int y)
            => ((y + tileMapRadius) / batchWidth) * batchesPerRow
             + ((x + tileMapRadius) / batchWidth);

        class Batch
        {
            private readonly Level level;
            private readonly GeometryLayer geometryLayer;
            private readonly (int X, int Y) baseTile;
            private readonly IndexedSurface<LevelVertex> surface;
            private readonly LevelGeometry geometry;

            private bool isDirty = true;

            public Batch(RenderContext context, Material material, Level level, GeometryLayer geometryLayer, (int X, int Y) baseTile)
            {
                this.level = level;
                this.geometryLayer = geometryLayer;
                this.baseTile = baseTile;
                surface = createSurface(context, material);
                
                geometry = new LevelGeometry(surface);
            }

            private static IndexedSurface<LevelVertex> createSurface(RenderContext context, Material material)
            {
                var surface = new IndexedSurface<LevelVertex>
                    {
                        ClearOnRender = false,
                        IsStatic = true
                    }
                    .WithShader(material.Shader.SurfaceShader)
                    .AndSettings(
                        context.Surfaces.ViewMatrix,
                        context.Surfaces.ProjectionMatrix,
                        context.Surfaces.FarPlaneDistance
                    );

                var textureUnit = TextureUnit.Texture0;

                foreach (var texture in material.ArrayTextures)
                {
                    surface.AddSetting(new ArrayTextureUniform(texture.UniformName, texture.Texture, textureUnit));

                    textureUnit++;
                }
                
                return surface;
            }

            public void MarkAsDirty()
            {
                isDirty = true;
            }

            public void Render()
            {
                if (isDirty)
                {
                    surface.Clear();
                    generateGeometry();
                    isDirty = false;
                }

                surface.Render();
            }

            private void generateGeometry()
            {
                for (var x = 0; x < batchWidth; x++)
                    for (var y = 0; y < batchWidth; y++)
                    {
                        var (tileX, tileY) = (baseTile.X + x, baseTile.Y + y);

                        var tile = new Tile(tileX, tileY);

                        if (!level.IsValid(tile))
                            continue;
                        
                        geometry.DrawTile(
                            level.GetPosition(tile).NumericValue,
                            geometryLayer[tile],
                            neighbourInfoOrDummy(tile, Direction.Right),
                            neighbourInfoOrDummy(tile, Direction.UpRight),
                            neighbourInfoOrDummy(tile, Direction.DownRight)
                        );
                    }

                // TODO: need to move LevelGeometry code here?
                // need to probably find a way to organise that code better
                // consider sharing a single level geometry instance across batches?
                // (need to inject surface then and pass that around internally, yuck)
            }
            
            private TileGeometry neighbourInfoOrDummy(Tile tile, Direction direction)
            {
                var neighbour = tile.Neighbour(direction);
                return level.IsValid(neighbour) ? geometryLayer[neighbour] : new TileGeometry();
            }
        }
    }
}
