using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.Buildings
{
    class BuildingBlueprint
    {
        private readonly List<Func<Component>> componentFactories;
        public TileSelection FootprintSelector { get; }
        public int MaxHealth { get; }

        public BuildingBlueprint(TileSelection footprint, int maxHealth,
            IEnumerable<Func<Component>> componentFactories)
        {
            this.componentFactories = componentFactories?.ToList();
            FootprintSelector = footprint;
            MaxHealth = maxHealth;
        }

        public IEnumerable<Component> GetComponents()
        {
            return componentFactories?.Select(f => f()) ?? Enumerable.Empty<Component>();
        }
    }
}
