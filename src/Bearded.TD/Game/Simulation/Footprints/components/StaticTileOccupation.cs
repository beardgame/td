using System.Collections.Generic;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Footprints
{
    sealed class StaticTileOccupation<T> : TileOccupationBase<T>
    {
        private readonly PositionedFootprint footprint;

        public override IEnumerable<Tile> OccupiedTiles => footprint.OccupiedTiles;

        public StaticTileOccupation(PositionedFootprint footprint)
        {
            this.footprint = footprint;
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            Events.Send(new FootprintChanged(footprint));
        }

        public override void Update(TimeSpan elapsedTime) {}
    }
}
