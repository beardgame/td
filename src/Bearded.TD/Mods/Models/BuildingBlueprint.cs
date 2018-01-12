using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components;

namespace Bearded.TD.Mods.Models
{
    sealed class BuildingBlueprint : IBlueprint
    {
        public string Id { get; }
        public string Name { get; }
        public FootprintGroup FootprintGroup { get; }
        public int MaxHealth { get; }
        public int ResourceCost { get; }

        public IReadOnlyList<IBuildingComponentFactory> ComponentFactories { get; }

        public BuildingBlueprint(string id, string name, FootprintGroup footprintGroup, int maxHealth,
            int resourceCost, IEnumerable<IBuildingComponentFactory> componentFactories)
        {
            Id = id;
            Name = name;
            FootprintGroup = footprintGroup;
            MaxHealth = maxHealth;
            ResourceCost = resourceCost;

            ComponentFactories = (componentFactories?.ToList() ?? new List<IBuildingComponentFactory>())
                .AsReadOnly();
        }

        public IEnumerable<IComponent<Building>> GetComponents()
            => throw new NotImplementedException();
    }
}
