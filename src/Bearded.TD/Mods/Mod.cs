using System.Collections.Generic;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Mods.Models;
using Bearded.TD.Tiles;

namespace Bearded.TD.Mods
{
    sealed class Mod
    {
        public string Name { get; }
        public string Id { get; }

        public ReadonlyBlueprintCollection<FootprintGroup> Footprints { get; }
        public ReadonlyBlueprintCollection<ComponentFactory> Components { get; }
        public ReadonlyBlueprintCollection<BuildingBlueprint> Buildings { get; }
        public ReadonlyBlueprintCollection<UnitBlueprint> Units { get; }

        public Mod()
        {
            Footprints = new ReadonlyBlueprintCollection<FootprintGroup>(new List<FootprintGroup>());
            Components = new ReadonlyBlueprintCollection<ComponentFactory>(new List<ComponentFactory>());
            Buildings = new ReadonlyBlueprintCollection<BuildingBlueprint>(new List<BuildingBlueprint>());
            Units = new ReadonlyBlueprintCollection<UnitBlueprint>(new List<UnitBlueprint>());
        }

        public Mod(
            ReadonlyBlueprintCollection<FootprintGroup> footprints,
            ReadonlyBlueprintCollection<ComponentFactory> components,
            ReadonlyBlueprintCollection<BuildingBlueprint> buildings,
            ReadonlyBlueprintCollection<UnitBlueprint> units)
        {
            Footprints = footprints;
            Components = components;
            Buildings = buildings;
            Units = units;
        }
    }
}
