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
            ReadonlyBlueprintCollection<FootprintGroup> footprints,
            ReadonlyBlueprintCollection<ComponentFactory> components,
            ReadonlyBlueprintCollection<BuildingBlueprint> buildings,
            ReadonlyBlueprintCollection<UnitBlueprint> units)
        {
            Blueprints = new Blueprints(footprints, components, buildings, units);
        }
    }
}
