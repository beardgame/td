using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Game.Simulation.Buildings
{
    interface IBuildingBlueprint : IBlueprint
    {
        string Name { get; }
        IReadOnlyList<UpgradeTag> Tags { get; }

        IEnumerable<IComponent<Building>> GetComponentsForBuilding();
        IEnumerable<IComponent<BuildingGhost>> GetComponentsForGhost();

        // TODO: these are ugly and we should figure out the correct way of doing it
        FootprintGroup GetFootprintGroup() =>
            GetComponentsForBuilding().OfType<IFootprintGroup>().SingleOrDefault()?.FootprintGroup ??
            FootprintGroup.Single;

        ResourceAmount GetResourceCost() =>
            GetComponentsForBuilding().OfType<ICost>().SingleOrDefault()?.Resources ?? ResourceAmount.Zero;
    }
}
