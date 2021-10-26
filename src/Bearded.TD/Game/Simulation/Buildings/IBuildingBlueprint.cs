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
        IEnumerable<IComponent<Building>> GetComponents();

        // TODO: these are ugly and we should figure out the correct way of doing it
        string GetName() =>
            GetComponents().OfType<INameProvider>().SingleOrDefault().NameOrDefault();

        FootprintGroup GetFootprintGroup() =>
            GetComponents().OfType<IFootprintGroup>().SingleOrDefault()?.FootprintGroup ??
            FootprintGroup.Single;

        ResourceAmount GetResourceCost() =>
            GetComponents().OfType<ICost>().SingleOrDefault()?.Resources ?? ResourceAmount.Zero;
    }
}
