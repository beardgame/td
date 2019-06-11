using System;
using System.Collections.Generic;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Resources;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Workers
{
    sealed class MiningTask : IWorkerTask
    {
        public string Name => "Mine a tile";
        public IEnumerable<Tile> Tiles => tile.Yield();
        public bool Finished => miningProgress >= Constants.Game.Worker.TotalMiningProgressRequired;

        private readonly MiningTaskPlaceholder miningTaskPlaceholder;
        private readonly Tile tile;
        private readonly GeometryLayer geometry;
        private readonly TileDrawInfo originalDrawInfo;

        private double miningProgress;

        public double PercentCompleted => miningProgress / Constants.Game.Worker.TotalMiningProgressRequired;

        public MiningTask(MiningTaskPlaceholder miningTaskPlaceholder, Tile tile, GeometryLayer geometry)
        {
            DebugAssert.Argument.Satisfies(geometry[tile].Type == TileType.Wall);

            this.miningTaskPlaceholder = miningTaskPlaceholder;
            this.tile = tile;
            this.geometry = geometry;
            originalDrawInfo = geometry[tile].DrawInfo;
        }

        public void Progress(TimeSpan elapsedTime, ResourceManager resourceManager, double ratePerS)
        {
            miningProgress += ratePerS * elapsedTime.NumericValue;
            if (Finished)
            {
                var tileGeo = new TileGeometry(TileType.Floor, geometry[tile].Geometry.Hardness);
                geometry.SetTileGeometry(tile, tileGeo, new TileDrawInfo(0.U(), originalDrawInfo.HexScale));
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

        public IRequest<GameInstance> CancelRequest()
        {
            if (miningProgress > 0)
                throw new InvalidOperationException("Cannot cancel a mining task after starting to mine.");
            return CancelMiningTask.Request(miningTaskPlaceholder);
        }
    }
}
