using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Game.Simulation.Buildings
{
    interface IBuildingBlueprint : IBlueprint
    {
        string Name { get; }
        FootprintGroup FootprintGroup { get; }
        ResourceAmount ResourceCost { get; }
        IReadOnlyList<UpgradeTag> Tags { get; }

        IEnumerable<IComponent<Building>> GetComponentsForBuilding();
        IEnumerable<IComponent<BuildingGhost>> GetComponentsForGhost();
    }
}
