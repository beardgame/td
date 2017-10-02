using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.Worker;

namespace Bearded.TD.Game.Resources
{
    class MiningTask : WorkerTask
    {
        public override Position2 Position => level.GetPosition(tile);
        public override bool Finished => miningProgress >= TotalMiningProgressRequired;

        private readonly Level level;
        private readonly Tile<TileInfo> tile;
        private readonly Unit originalTileHeight;

        private double miningProgress;

        public MiningTask(Level level, Tile<TileInfo> tile)
        {
            DebugAssert.Argument.Satisfies(tile.Info.TileType == TileInfo.Type.Wall);

            this.level = level;
            this.tile = tile;
            originalTileHeight = tile.Info.DrawInfo.Height;
        }

        public override void Progress(TimeSpan elapsedTime, ResourceManager resourceManager, double ratePerS)
        {
            miningProgress += ratePerS * elapsedTime.NumericValue;
            if (Finished)
            {
                tile.Info.SetDrawInfo(new TileDrawInfo(0.U(), tile.Info.DrawInfo.HexScale));
                tile.Info.SetTileType(TileInfo.Type.Floor);
            }
            else
            {
                tile.Info.SetDrawInfo(
                    new TileDrawInfo(originalTileHeight * (float)(1 - miningProgress / TotalMiningProgressRequired),
                    tile.Info.DrawInfo.HexScale));
            }
        }
    }
}
