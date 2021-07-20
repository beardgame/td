using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class StaticTileOccupation<T> : TileOccupationBase<T>
    {
        private readonly PositionedFootprint footprint;

        public override IEnumerable<Tile> OccupiedTiles => footprint.OccupiedTiles;

        public StaticTileOccupation(PositionedFootprint footprint)
        {
            this.footprint = footprint;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Events.Send(new FootprintChanged(footprint));
        }

        public override void Update(TimeSpan elapsedTime) {}

        public override void Draw(CoreDrawers drawers) {}
    }
}
