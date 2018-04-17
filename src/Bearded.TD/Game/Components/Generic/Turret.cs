﻿using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Weapons;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    interface ITurret : IPositionable
    {
        GameObject Owner { get; }
        Faction OwnerFaction { get; }
    }

    [Component("turret")]
    class Turret<T> : Component<T, TurretParameters>, ITurret
        where T : BuildingBase<T>
    {
        private Weapon weapon;

        public Position2 Position => Owner.Position + Parameters.Offset;

        public Turret(TurretParameters parameters) : base(parameters) { }

        protected override void Initialise()
        {
            weapon = new Weapon(Parameters.Weapon, this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            weapon.Update(elapsedTime);
        }
        
        public override void Draw(GeometryManager geometries)
        {
            weapon.Draw(geometries);
        }

        GameObject ITurret.Owner => Owner;
        Faction ITurret.OwnerFaction => Owner.Faction;
    }
}