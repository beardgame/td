using System.Collections.Generic;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Game.World;

namespace Bearded.TD.Game.Buildings
{
    interface IBuildingBlueprint : IBlueprint, IAttributeModifiable
    {
        string Name { get; }
        FootprintGroup FootprintGroup { get; }
        int ResourceCost { get; }
        IReadOnlyList<UpgradeTag> Tags { get; }

        IEnumerable<IComponent<Building>> GetComponentsForBuilding();
        IEnumerable<IComponent<BuildingGhost>> GetComponentsForGhost();
        IEnumerable<IComponent<BuildingPlaceholder>> GetComponentsForPlaceholder();

        IBuildingBlueprint MakeModifiableInstance();
    }
}
