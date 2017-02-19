using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Buildings
{
    class BuildingGhost : GameObject
    {
        private readonly BuildingBlueprint blueprint;
        private PositionedFootprint footprint;

        public BuildingGhost(BuildingBlueprint blueprint)
        {
            this.blueprint = blueprint;
        }

        public void SetFootprint(PositionedFootprint footprint)
        {
            this.footprint = footprint;
        }

        public override void Update(TimeSpan elapsedTime)
        { }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;

            foreach (var tile in footprint.OccupiedTiles)
            {
                if (!tile.IsValid) continue;

                geo.Color = tile.Info.IsPassable ? Color.Green : Color.Red;
                geo.DrawCircle(Game.Level.GetPosition(tile).NumericValue, .2f);
            }
        }
    }
}
