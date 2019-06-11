using System.Collections.Generic;
using Bearded.TD.Game.Resources;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Workers
{
    sealed class MiningTask : IWorkerTask
    {
        public Id<IWorkerTask> Id { get; }
        public string Name => "Mine a tile";
        public IEnumerable<Tile> Tiles => tile.Yield();
        public bool CanAbort => miningProgress == 0;
        public bool Finished => miningProgress >= Constants.Game.Worker.TotalMiningProgressRequired;

        private readonly MiningTaskPlaceholder miningTaskPlaceholder;
        private readonly Tile tile;
        private readonly GeometryLayer geometry;
        private readonly TileDrawInfo originalDrawInfo;

        private double miningProgress;

        public double PercentCompleted => miningProgress / Constants.Game.Worker.TotalMiningProgressRequired;

        public MiningTask(
            Id<IWorkerTask> id, MiningTaskPlaceholder miningTaskPlaceholder, Tile tile, GeometryLayer geometry)
        {
            DebugAssert.Argument.Satisfies(geometry[tile].Type == TileType.Wall);

            Id = id;
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

        public void OnAbort()
        {
            miningTaskPlaceholder.Delete();
        }
    }
}
