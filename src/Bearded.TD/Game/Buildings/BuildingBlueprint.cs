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
        public TileSelection FootprintSelector { get; }
        public int MaxHealth { get; }
        public int ResourceCost { get; }
        private readonly List<Func<Component>> componentFactories;

        public BuildingBlueprint(Id<BuildingBlueprint> id, string name, TileSelection footprint, int maxHealth,
            int resourceCost, IEnumerable<Func<Component>> componentFactories)
        {
            Id = id;
            Name = name;
            FootprintSelector = footprint;
            MaxHealth = maxHealth;
            ResourceCost = resourceCost;
            this.componentFactories = componentFactories?.ToList();
        }

        public IEnumerable<Component> GetComponents()
        {
            return componentFactories?.Select(f => f()) ?? Enumerable.Empty<Component>();
        }
    }
}
