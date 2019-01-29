using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Weapons;

namespace Bearded.TD.Content.Models
{
    sealed class WeaponBlueprint : IWeaponBlueprint
    {
        public string Id { get; }
        private readonly IReadOnlyList<IComponentFactory<Weapon>> componentFactories;

        public IEnumerable<IComponent<Weapon>> GetComponents()
            => componentFactories.Select(f => f.Create());

        public WeaponBlueprint(string id, IEnumerable<IComponentFactory<Weapon>> componentFactories)
        {
            Id = id;

            this.componentFactories = (componentFactories?.ToList() ?? new List<IComponentFactory<Weapon>>())
                    .AsReadOnly();
        }
    }
}
