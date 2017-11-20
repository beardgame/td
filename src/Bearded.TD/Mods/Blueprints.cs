using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Components;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Mods
{
    sealed class Blueprints
    {
        public ReadonlyBlueprintCollection<FootprintGroup> Footprints { get; }
        public ReadonlyBlueprintCollection<ComponentFactory> Components { get; }
        public ReadonlyBlueprintCollection<BuildingBlueprint> Buildings { get; }
        public ReadonlyBlueprintCollection<UnitBlueprint> Units { get; }

        public Blueprints(
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

        public static Blueprints Merge(IEnumerable<Blueprints> blueprints)
        {
            var blueprintsList = blueprints as IList<Blueprints> ?? blueprints.ToList();
            return new Blueprints(
                new ReadonlyBlueprintCollection<FootprintGroup>(
                    blueprintsList.SelectMany(blueprint => blueprint.Footprints.All)),
                new ReadonlyBlueprintCollection<ComponentFactory>(
                    blueprintsList.SelectMany(blueprint => blueprint.Components.All)),
                new ReadonlyBlueprintCollection<BuildingBlueprint>(
                    blueprintsList.SelectMany(blueprint => blueprint.Buildings.All)),
                new ReadonlyBlueprintCollection<UnitBlueprint>(
                    blueprintsList.SelectMany(blueprint => blueprint.Units.All)));
        }
    }
}
