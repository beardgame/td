using Bearded.TD.Game.Buildings;
using Bearded.TD.Mods.Models;
using Bearded.TD.Tiles;

namespace Bearded.TD.Mods
{
    sealed class Mod
    {
        public string Name { get; }
        public string Id { get; }

        public ReadonlyBlueprintCollection<Footprint> Footprints { get; }
        public ReadonlyNamedBlueprintCollection<ComponentFactory> Components { get; }
        public ReadonlyNamedBlueprintCollection<BuildingBlueprint> Buildings { get; }
        public ReadonlyNamedBlueprintCollection<UnitBlueprint> Units { get; }

        public Mod()
        {
            
        }
    }
}
