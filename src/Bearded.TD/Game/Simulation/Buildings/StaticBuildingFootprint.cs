using System.Collections.Generic;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class StaticBuildingFootprint<T> : BuildingFootprintBase<T>
    {
        private readonly PositionedFootprint footprint;

        public override IEnumerable<Tile> OccupiedTiles => footprint.OccupiedTiles;

        public StaticBuildingFootprint(PositionedFootprint footprint)
        {
            this.footprint = footprint;
        }

        protected override IEnumerable<Tile> GetOccupiedTiles() => footprint.OccupiedTiles;

        public override void Update(TimeSpan elapsedTime) {}

        public override void Draw(CoreDrawers drawers) {}
    }
}
