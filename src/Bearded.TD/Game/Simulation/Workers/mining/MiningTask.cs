using System.Collections.Generic;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Workers
{
    sealed class MiningTask : IWorkerTask
    {
        public Id<IWorkerTask> Id { get; }
        public string Name => "Mine a tile";
        public IEnumerable<Tile> Tiles => tile.Yield();
        public Maybe<ISelectable> Selectable => Maybe.Just<ISelectable>(miningTaskPlaceholder);
        public bool CanAbort => miningProgress == ResourceAmount.Zero;
        public bool Finished => miningProgress >= Constants.Game.Worker.TotalMiningProgressRequired;

        private readonly MiningTaskPlaceholder miningTaskPlaceholder;
        private readonly Tile tile;
        private readonly GeometryLayer geometry;
        private readonly TileDrawInfo originalDrawInfo;

        private ResourceAmount miningProgress;

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

        public void Progress(TimeSpan elapsedTime, ResourceManager resourceManager, ResourceRate ratePerS)
        {
            // TODO: mining progress shouldn't be measured in resources
            miningProgress += ratePerS * elapsedTime;
            if (Finished)
            {
                var previousGeo = geometry[tile].Geometry;
                var tileGeo = new TileGeometry(TileType.Floor, previousGeo.Hardness, previousGeo.FloorHeight);
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
