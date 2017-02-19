using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Buildings
{
    class BuildingGhost : GameObject
    {
        private readonly BuildingBlueprint blueprint;
        private Tile<TileInfo> rootTile;

        public BuildingGhost(BuildingBlueprint blueprint)
        {
            this.blueprint = blueprint;
        }

        public void SetRootTile(Tile<TileInfo> tile)
        {
            rootTile = tile;
        }

        public override void Update(TimeSpan elapsedTime)
        { }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;

            foreach (var tile in blueprint.Footprint.OccupiedTiles(rootTile))
            {
                if (!tile.IsValid) continue;

                geo.Color = tile.Info.IsPassable ? Color.Green : Color.Red;
                geo.DrawCircle(Game.Level.GetPosition(tile).NumericValue, .1f);
            }
        }
    }
}
