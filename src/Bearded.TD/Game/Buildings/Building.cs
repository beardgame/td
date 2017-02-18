using System;
using System.Collections.Generic;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Buildings
{
    abstract class Building : GameObject
    {
        private readonly BuildingBlueprint blueprint;
        private readonly Tile<TileInfo> rootTile;

        protected Position2 Position { get; private set; }
        protected int Health { get; private set; }
        protected IEnumerable<Tile<TileInfo>> OccupiedTiles => blueprint.Footprint.OccupiedTiles(rootTile);

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
            OccupiedTiles.ForEach((tile) => Game.Geometry.SetBuilding(tile, this));
        }

        protected override void OnDelete()
        {
            OccupiedTiles.ForEach((tile) => Game.Geometry.SetBuilding(tile, null));
        }
    }
}
