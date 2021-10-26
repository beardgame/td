using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Game.Simulation.Buildings
{
    static class BuildingBlueprintExtensions
    {
        // TODO: these are ugly and we should figure out the correct way of doing it
        public static string GetName<TOwner>(this IComponentOwnerBlueprint blueprint) =>
            blueprint.GetComponents<TOwner>().OfType<INameProvider>().SingleOrDefault().NameOrDefault();

        public static FootprintGroup GetFootprintGroup<TOwner>(this IComponentOwnerBlueprint blueprint) =>
            blueprint.GetComponents<TOwner>().OfType<IFootprintGroup>().SingleOrDefault()?.FootprintGroup ??
            FootprintGroup.Single;

        public static ResourceAmount GetResourceCost<TOwner>(this IComponentOwnerBlueprint blueprint) =>
            blueprint.GetComponents<TOwner>().OfType<ICost>().SingleOrDefault()?.Resources ?? ResourceAmount.Zero;
    }
}
