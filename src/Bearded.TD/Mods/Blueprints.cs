using Bearded.TD.Game.Buildings;
using Bearded.TD.Mods.Models;
using Bearded.TD.Tiles;

namespace Bearded.TD.Mods
{
    sealed class Blueprints
    {
        public BlueprintCollection<FootprintGroup> Footprints { get; } = new BlueprintCollection<FootprintGroup>();
        public BlueprintCollection<ComponentFactory> Components { get; } = new BlueprintCollection<ComponentFactory>();
        public BlueprintCollection<BuildingBlueprint> Buildings { get; } = new BlueprintCollection<BuildingBlueprint>();
        public BlueprintCollection<UnitBlueprint> Units { get; } = new BlueprintCollection<UnitBlueprint>();
    }
}
