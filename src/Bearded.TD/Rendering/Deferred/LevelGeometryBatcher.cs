using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.World;

namespace Bearded.TD.Rendering.Deferred
{
    class LevelGeometryBatcher
    {
        private const int batchWidth = 8;

        private readonly int tileMapRadius;
        private readonly int batchesPerRow;
        private readonly Batch[] batchLookup;
        private readonly List<Batch> batches = new List<Batch>();

        private readonly Level level;
        
        // TODO: actually create this on game creation (in GameWorldControl?)
        // TODO: in the future this also needs to know about textures and stuff from mods
        public LevelGeometryBatcher(GameInstance game, RenderContext context)
        {
            level = game.State.Level;
            tileMapRadius = level.Tilemap.Radius;

            var tileMapWidth = tileMapRadius * 2 + 1;

            batchesPerRow = (tileMapWidth + batchWidth - 1) / batchWidth;

            batchLookup = new Batch[batchesPerRow * batchesPerRow];

            createValidBatches(context);
        }

        // TODO: call this anytime a tile is changed
        public void MarkDirty((int X, int Y) tile)
            => batchLookup[batchIndexFor(tile)].MarkAsDirty();

        // TODO: call this from deferred renderer
        public void RenderAll()
        {
            foreach (var batch in batches)
            {
                batch.Render();
            }
        }

        private void createValidBatches(RenderContext context)
        {
            foreach (var i in Enumerable
                .Range(0, batchLookup.Length)
                .Where(isValidBatch))
            {
                var baseTile = baseTileFor(i);
                var batch = new Batch(context, level, baseTile);

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
                    if (level.Tilemap.IsValidTile(baseTile.X + x, baseTile.Y + y))
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
            private readonly (int X, int Y) baseTile;
            private readonly IndexedSurface<LevelVertex> surface;

            private bool isDirty = true;

            public Batch(RenderContext context, Level level, (int X, int Y) baseTile)
            {
                this.baseTile = baseTile;
                surface = createSurface(context);
            }

            private static IndexedSurface<LevelVertex> createSurface(RenderContext context)
            {
                return new IndexedSurface<LevelVertex>
                    {
                        ClearOnRender = false,
                        IsStatic = true
                    }
                    .WithShader(context.Surfaces.Shaders["deferred/gLevel"])
                    .AndSettings(
                        context.Surfaces.ViewMatrix,
                        context.Surfaces.ProjectionMatrix,
                        context.Surfaces.FarPlaneDistance
                    );
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
                }

                surface.Render();
            }

            private void generateGeometry()
            {
                for (var x = 0; x < batchWidth; x++)
                    for (var y = 0; y < batchWidth; y++)
                    {
                        var (tileX, tileY) = (baseTile.X + x, baseTile.Y + y);

                        // TODO: create tile vertices
                    }

                // TODO: need to move LevelGeometry code here? need to probably find a way to organise that code better
            }
        }
    }
}
