using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Game.Simulation.Buildings
{
    interface IBuildingBlueprint : IBlueprint
    {
        IEnumerable<IComponent<Building>> GetComponentsForBuilding();
        IEnumerable<IComponent<BuildingGhost>> GetComponentsForGhost();

        // TODO: these are ugly and we should figure out the correct way of doing it
        string GetName() =>
            GetComponentsForBuilding().OfType<INameProvider>().SingleOrDefault().NameOrDefault();

        FootprintGroup GetFootprintGroup() =>
            GetComponentsForBuilding().OfType<IFootprintGroup>().SingleOrDefault()?.FootprintGroup ??
            FootprintGroup.Single;

        ResourceAmount GetResourceCost() =>
            GetComponentsForBuilding().OfType<ICost>().SingleOrDefault()?.Resources ?? ResourceAmount.Zero;
    }
}
