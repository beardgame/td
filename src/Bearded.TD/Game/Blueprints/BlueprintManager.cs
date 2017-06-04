using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.Blueprints
{
    class BlueprintManager
    {
        public BlueprintCollection<Footprint> Footprints { get; } = new BlueprintCollection<Footprint>();
        public NamedBlueprintCollection<ComponentFactory> Components { get; } = new NamedBlueprintCollection<ComponentFactory>();
        public NamedBlueprintCollection<BuildingBlueprint> Buildings { get; } = new NamedBlueprintCollection<BuildingBlueprint>();
    }
}
