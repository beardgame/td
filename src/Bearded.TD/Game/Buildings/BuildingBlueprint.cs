using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Buildings
{
    class BuildingBlueprint : IIdable<BuildingBlueprint>, INamed
    {
        public Id<BuildingBlueprint> Id { get; }
        public string Name { get; }
        public FootprintGroup Footprints { get; }
        public int MaxHealth { get; }
        public int ResourceCost { get; }
        private readonly List<ComponentFactory> componentFactories;

        public IReadOnlyList<ComponentFactory> ComponentFactories => componentFactories?.AsReadOnly()
            ?? (IReadOnlyList<ComponentFactory>) Array.Empty<ComponentFactory>();

        public BuildingBlueprint(Id<BuildingBlueprint> id, string name, FootprintGroup footprints, int maxHealth,
            int resourceCost, IEnumerable<ComponentFactory> componentFactories)
        {
            Id = id;
            Name = name;
            Footprints = footprints;
            MaxHealth = maxHealth;
            ResourceCost = resourceCost;
            this.componentFactories = componentFactories?.ToList();
        }

        public IEnumerable<Component> GetComponents()
        {
            return componentFactories?.Select(f => f.Create()) ?? Enumerable.Empty<Component>();
        }
    }
}
