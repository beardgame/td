using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.World;

namespace Bearded.TD.Rendering.Deferred
{
    class LevelGeometryBatcher
    {
        private const int batchWidth = 8;

        private readonly int tileMapRadius;
        private readonly int batchesPerRow;
        private readonly Batch[] batches;

        private readonly Level level;

        private readonly HashSet<int> dirtyBatches = new HashSet<int>();

        // TODO: inject render context for surface creation
        // TODO: actually create this on game creation (in GameWorldControl?)
        // TODO: in the future this also needs to know about textures and stuff
        public LevelGeometryBatcher(GameInstance game)
        {
            level = game.State.Level;
            tileMapRadius = level.Tilemap.Radius;

            var tileMapWidth = tileMapRadius * 2 + 1;

            batchesPerRow = (tileMapWidth + batchWidth - 1) / batchWidth;

            batches = new Batch[batchesPerRow * batchesPerRow];

            createValidBatches();
        }

        // TODO: method to render all batches

        private void createValidBatches()
        {
            foreach (var i in Enumerable
                .Range(0, batches.Length)
                .Where(isValidBatch))
            {
                batches[i] = new Batch();
                dirtyBatches.Add(i);
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

        // TODO: call this anytime a tile is changed
        public void MarkDirty((int X, int Y) tile)
            => dirtyBatches.Add(batchIndexFor(tile));

        private int batchIndexFor((int x, int y) tile)
            => batchIndexFor(tile.x, tile.y);

        private int batchIndexFor(int x, int y)
            => ((y + tileMapRadius) / batchWidth) * batchesPerRow
             + ((x + tileMapRadius) / batchWidth);

        class Batch
        {

            public Batch()
            {
                // TODO: create surface, etc.
            }

            // TODO: method to recreate vertices
            // TODO: need to move LevelGeometry code here? need to probably find a way to organise that code better
            // TODO: method to render surface
        }
    }
}
