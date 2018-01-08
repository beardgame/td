using System;
using System.Collections.Generic;
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
        private readonly List<ComponentFactory> componentFactories;

        public IReadOnlyList<ComponentFactory> ComponentFactories => componentFactories?.AsReadOnly()
            ?? (IReadOnlyList<ComponentFactory>) Array.Empty<ComponentFactory>();

        public BuildingBlueprint(string id, string name, FootprintGroup footprintGroup, int maxHealth,
            int resourceCost, IEnumerable<ComponentFactory> componentFactories)
        {
            Id = id;
            Name = name;
            FootprintGroup = footprintGroup;
            MaxHealth = maxHealth;
            ResourceCost = resourceCost;
            this.componentFactories = componentFactories?.ToList();
        }

        public IEnumerable<IComponent<Building>> GetComponents()
            => componentFactories?.Select(f => f.Create()) ?? Enumerable.Empty<IComponent<Building>>();
    }
}
