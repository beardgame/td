using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Weapons;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Mods.Models
{
    sealed class WeaponBlueprint : IWeaponBlueprint
    {
        public string Id { get; }
        public Unit Range { get; }
        public TimeSpan ReCalculateTilesInRangeInterval { get; }
        public TimeSpan NoTargetIdleInterval { get; }

        private readonly IReadOnlyList<IComponentFactory<Weapon>> componentFactories;

        public IEnumerable<IComponent<Weapon>> GetComponents()
            => componentFactories.Select(f => f.Create());

        public WeaponBlueprint(string id, Unit range, TimeSpan reCalculateTilesInRangeInterval,
            TimeSpan noTargetIdleInterval, IEnumerable<IComponentFactory<Weapon>> componentFactories)
        {
            Id = id;
            ReCalculateTilesInRangeInterval = reCalculateTilesInRangeInterval;
            Range = range;
            NoTargetIdleInterval = noTargetIdleInterval;

            this.componentFactories = (componentFactories?.ToList() ?? new List<IComponentFactory<Weapon>>())
                    .AsReadOnly();
        }
    }
}
