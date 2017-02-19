﻿using System;
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
        private PositionedFootprint footprint;

        protected Position2 Position { get; private set; }
        protected int Health { get; private set; }
        public IEnumerable<Tile<TileInfo>> OccupiedTiles => footprint.OccupiedTiles;

        protected List<Component> Components { get; } = new List<Component>();

        protected Building(BuildingBlueprint blueprint, PositionedFootprint footprint)
        {
            if (!footprint.IsValid) throw new ArgumentOutOfRangeException();

            this.blueprint = blueprint;
            this.footprint = footprint;
            Health = blueprint.MaxHealth;

            blueprint.GetComponents().ForEach(Components.Add);
        }

        public void Damage(int damage)
        {
            Health -= damage;
            OnDamaged();
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Position = footprint.CenterPosition;
            OccupiedTiles.ForEach((tile) => Game.Geometry.SetBuilding(tile, this));
            Components.ForEach(c => c.OnAdded(this));
        }

        protected override void OnDelete()
        {
            OccupiedTiles.ForEach((tile) => Game.Geometry.SetBuilding(tile, null));
        }

        protected virtual void OnDamaged()
        { }
    }
}
