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
        public static string GetName(this IComponentOwnerBlueprint blueprint) =>
            blueprint.GetComponents<ComponentGameObject>().OfType<INameProvider>().SingleOrDefault().NameOrDefault();

        public static FootprintGroup GetFootprintGroup(this IComponentOwnerBlueprint blueprint) =>
            blueprint.GetComponents<ComponentGameObject>().OfType<IFootprintGroup>().SingleOrDefault()?.FootprintGroup ??
            FootprintGroup.Single;

        public static ResourceAmount GetResourceCost(this IComponentOwnerBlueprint blueprint) =>
            blueprint.GetComponents<ComponentGameObject>().OfType<ICost>().SingleOrDefault()?.Resources ?? ResourceAmount.Zero;
    }
}
