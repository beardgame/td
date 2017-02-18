using System;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Buildings
{
    abstract class Building : GameObject
    {
        private readonly BuildingBlueprint blueprint;
        private readonly Tile<TileInfo> rootTile;

        protected Position2 Position { get; private set; }
        protected int Health { get; private set; }

        protected Building(BuildingBlueprint blueprint, Tile<TileInfo> rootTile)
        {
            if (!rootTile.IsValid) throw new ArgumentOutOfRangeException();

            this.blueprint = blueprint;
            this.rootTile = rootTile;
            Health = blueprint.MaxHealth;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Position = blueprint.Footprint.Center(Game.Level, rootTile);
            foreach (var tile in blueprint.Footprint.OccupiedTiles(rootTile))
            {
                tile.Info.SetBuilding(this);
                Game.Geometry.UpdatePassability(tile);
                Game.Navigator.AddSink(tile);
            }
        }

        protected override void OnDelete()
        {
            foreach (var tile in blueprint.Footprint.OccupiedTiles(rootTile))
            {
                tile.Info.SetBuilding(null);
                Game.Geometry.UpdatePassability(tile);
                // TODO: remove sink
            }
        }
    }
}
