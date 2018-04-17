using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Weapons;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Mods.Models
{
    sealed class WeaponBlueprint : IBlueprint
    {
        public string Id { get; }
        public TimeSpan ShootInterval { get; }
        public TimeSpan IdleInterval { get; }
        public TimeSpan ReCalculateTilesInRangeInterval { get; }
        public Unit Range { get; }
        public int Damage { get; }

        private readonly IReadOnlyList<IComponentFactory<Weapon>> componentFactories;

        public IEnumerable<IComponentFactory<Weapon>> Components => componentFactories;

        public WeaponBlueprint(
                string id,
                TimeSpan shootInterval,
                TimeSpan idleInterval,
                TimeSpan reCalculateTilesInRangeInterval,
                Unit range,
                int damage,
                IEnumerable<IComponentFactory<Weapon>> componentFactories)
        {
            Id = id;
            ShootInterval = shootInterval;
            IdleInterval = idleInterval;
            ReCalculateTilesInRangeInterval = reCalculateTilesInRangeInterval;
            Range = range;
            Damage = damage;

            this.componentFactories = (componentFactories?.ToList() ?? new List<IComponentFactory<Weapon>>())
                    .AsReadOnly();
        }
    }
}
