using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Game.Blueprints
{
    class BlueprintManager
    {
        public BlueprintCollection<FootprintGroup> Footprints { get; } = new BlueprintCollection<FootprintGroup>();
        public BlueprintCollection<ComponentFactory> Components { get; } = new BlueprintCollection<ComponentFactory>();
        public BlueprintCollection<BuildingBlueprint> Buildings { get; } = new BlueprintCollection<BuildingBlueprint>();
    }
}
