using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Buildings
{
    abstract class Building : GameObject
    {
        private readonly Tile<TileInfo> rootTile;
        private readonly Blueprint blueprint;

        protected Position2 Position { get; private set; }
        protected int Health { get; private set; }

        protected Building(Tile<TileInfo> rootTile, Blueprint blueprint)
        {
            this.rootTile = rootTile;
            this.blueprint = blueprint;
            Health = blueprint.MaxHealth;
        }

        protected override void OnAdded()
        {
            Position = blueprint.Footprint.Center(Game.Level, rootTile);
            foreach (var tile in blueprint.Footprint.OccupiedTiles(rootTile))
            {
                var info = tile.Info;
                info.SetBuilding(this);
                Game.Geometry.UpdatePassability(tile, info);
                Game.Navigator.AddSink(tile);
            }
        }

        protected override void OnDelete()
        {
            foreach (var tile in blueprint.Footprint.OccupiedTiles(rootTile))
            {
                var info = tile.Info;
                info.SetBuilding(null);
                Game.Geometry.UpdatePassability(tile, info);
                // TODO: remove sink
            }
        }
    }
}
