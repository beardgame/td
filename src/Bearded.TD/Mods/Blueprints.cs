using Bearded.TD.Game.Buildings;
using Bearded.TD.Mods.Models;
using Bearded.TD.Tiles;

namespace Bearded.TD.Mods
{
    sealed class Blueprints
    {
        public BlueprintCollection<Footprint> Footprints { get; } = new BlueprintCollection<Footprint>();
        public NamedBlueprintCollection<ComponentFactory> Components { get; } = new NamedBlueprintCollection<ComponentFactory>();
        public NamedBlueprintCollection<BuildingBlueprint> Buildings { get; } = new NamedBlueprintCollection<BuildingBlueprint>();

        public NamedBlueprintCollection<UnitBlueprint> Units { get; } = new NamedBlueprintCollection<UnitBlueprint>();
    }
}
