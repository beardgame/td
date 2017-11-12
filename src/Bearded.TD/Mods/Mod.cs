﻿using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Mods
{
    sealed class Mod
    {
        public string Name { get; }
        public string Id { get; }

        public Blueprints Blueprints { get; }

        public Mod(
            IEnumerable<FootprintGroup> footprints = null,
            IEnumerable<ComponentFactory> components = null,
            IEnumerable<BuildingBlueprint> buildings = null,
            IEnumerable<UnitBlueprint> units = null)
            : this(
                wrap(footprints),
                wrap(components),
                wrap(buildings),
                wrap(units)
            )
        { }

        public Mod(
            ReadonlyBlueprintCollection<FootprintGroup> footprints,
            ReadonlyBlueprintCollection<ComponentFactory> components,
            ReadonlyBlueprintCollection<BuildingBlueprint> buildings,
            ReadonlyBlueprintCollection<UnitBlueprint> units)
        {
            Blueprints = new Blueprints(footprints, components, buildings, units);
        }

        private static ReadonlyBlueprintCollection<T> wrap<T>(IEnumerable<T> blueprints)
            where T : IBlueprint
            => new ReadonlyBlueprintCollection<T>(blueprints ?? Enumerable.Empty<T>());
    }
}
