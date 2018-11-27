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
        public override IEnumerable<Tile> Tiles => tile.Yield();
        public override bool Finished => miningProgress >= Constants.Game.Worker.TotalMiningProgressRequired;

        private readonly Tile tile;
        private readonly GeometryLayer geometry;
        private readonly TileDrawInfo originalDrawInfo;

        private double miningProgress;

        public MiningTask(Level level, Tile tile, GeometryLayer geometry)
        {
            DebugAssert.Argument.Satisfies(geometry[tile].Type == TileGeometry.TileType.Wall);

            this.tile = tile;
            this.geometry = geometry;
            originalDrawInfo = geometry[tile].DrawInfo;
        }

        public override void Progress(TimeSpan elapsedTime, ResourceManager resourceManager, double ratePerS)
        {
            miningProgress += ratePerS * elapsedTime.NumericValue;
            if (Finished)
            {
                geometry.SetTileType(tile, TileGeometry.TileType.Floor, new TileDrawInfo(0.U(), originalDrawInfo.HexScale));
            }
            else
            {
                geometry.SetDrawInfo(tile,
                    new TileDrawInfo(
                        originalDrawInfo.Height * (float)(1 - miningProgress / Constants.Game.Worker.TotalMiningProgressRequired),
                        originalDrawInfo.HexScale)
                );
            }
        }
    }
}
