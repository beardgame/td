using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Game.Simulation.Buildings;

static class BuildingBlueprintExtensions
{
    // TODO: these are ugly and we should figure out the correct way of doing it
    public static string GetName(this IComponentOwnerBlueprint blueprint) =>
        blueprint.GetComponents().OfType<INameProvider>().SingleOrDefault().NameOrDefault();

    public static World.FootprintGroup GetFootprintGroup(this IComponentOwnerBlueprint blueprint) =>
        blueprint.GetComponents().OfType<IFootprintGroup>().SingleOrDefault()?.FootprintGroup ??
        World.FootprintGroup.Single;

    public static ResourceAmount GetResourceCost(this IComponentOwnerBlueprint blueprint) =>
        blueprint.GetComponents().OfType<ICost>().SingleOrDefault()?.Resources ?? ResourceAmount.Zero;
}
