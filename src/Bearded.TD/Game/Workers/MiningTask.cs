using System.Collections.Generic;
using Bearded.TD.Game.Resources;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Workers
{
    class MiningTask : WorkerTask
    {
        public override IEnumerable<Tile<TileInfo>> Tiles => tile.Yield();
        public override bool Finished => miningProgress >= Constants.Game.Worker.TotalMiningProgressRequired;

        private readonly Level level;
        private readonly Tile<TileInfo> tile;
        private readonly LevelGeometry geometry;
        private readonly Unit originalTileHeight;

        private double miningProgress;

        public MiningTask(Level level, Tile<TileInfo> tile, LevelGeometry geometry)
        {
            DebugAssert.Argument.Satisfies(tile.Info.IsMineable);

            this.level = level;
            this.tile = tile;
            this.geometry = geometry;
            originalTileHeight = tile.Info.DrawInfo.Height;
        }

        public override void Progress(TimeSpan elapsedTime, ResourceManager resourceManager, double ratePerS)
        {
            miningProgress += ratePerS * elapsedTime.NumericValue;
            if (Finished)
            {
                geometry.SetTileType(tile, TileInfo.Type.Floor, new TileDrawInfo(0.U(), tile.Info.DrawInfo.HexScale));
            }
            else
            {
                geometry.SetDrawInfo(tile,
                    new TileDrawInfo(
                        originalTileHeight * (float)(1 - miningProgress / Constants.Game.Worker.TotalMiningProgressRequired),
                        tile.Info.DrawInfo.HexScale)
                );
            }
        }
    }
}
