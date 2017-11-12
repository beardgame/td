using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Tiles;

namespace Bearded.TD.Mods.Models
{
    sealed class BuildingBlueprint : IBlueprint
    {
        public string Name { get; }
        public FootprintGroup Footprints { get; }
        public int MaxHealth { get; }
        public int ResourceCost { get; }
        private readonly List<ComponentFactory> componentFactories;

        public IReadOnlyList<ComponentFactory> ComponentFactories => componentFactories?.AsReadOnly()
            ?? (IReadOnlyList<ComponentFactory>) Array.Empty<ComponentFactory>();

        public BuildingBlueprint(string name, FootprintGroup footprints, int maxHealth,
            int resourceCost, IEnumerable<ComponentFactory> componentFactories)
        {
            Name = name;
            Footprints = footprints;
            MaxHealth = maxHealth;
            ResourceCost = resourceCost;
            this.componentFactories = componentFactories?.ToList();
        }

        public IEnumerable<Component> GetComponents()
            => componentFactories?.Select(f => f.Create()) ?? Enumerable.Empty<Component>();
    }
}
