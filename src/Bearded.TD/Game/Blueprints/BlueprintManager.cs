using Bearded.TD.Game.Buildings;

namespace Bearded.TD.Game.Blueprints
{
    class BlueprintManager
    {
        public BlueprintCollection<BuildingBlueprint> Buildings { get; } = new BlueprintCollection<BuildingBlueprint>();
    }
}
