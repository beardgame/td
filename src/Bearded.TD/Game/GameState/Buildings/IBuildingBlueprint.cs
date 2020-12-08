using System.Collections.Generic;
using Bearded.TD.Game.GameState.Components;
using Bearded.TD.Game.GameState.Resources;
using Bearded.TD.Game.GameState.Upgrades;
using Bearded.TD.Game.GameState.World;

namespace Bearded.TD.Game.GameState.Buildings
{
    interface IBuildingBlueprint : IBlueprint
    {
        string Name { get; }
        FootprintGroup FootprintGroup { get; }
        ResourceAmount ResourceCost { get; }
        IReadOnlyList<UpgradeTag> Tags { get; }

        IEnumerable<IComponent<Building>> GetComponentsForBuilding();
        IEnumerable<IComponent<BuildingGhost>> GetComponentsForGhost();
        IEnumerable<IComponent<BuildingPlaceholder>> GetComponentsForPlaceholder();
    }
}
