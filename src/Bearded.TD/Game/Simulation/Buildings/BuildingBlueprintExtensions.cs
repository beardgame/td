using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Game.Simulation.Buildings;

static class BuildingBlueprintExtensions
{
    // TODO: these are ugly and we should figure out the correct way of doing it
    public static string GetName(this IGameObjectBlueprint blueprint) =>
        blueprint.GetComponents().OfType<INameProvider>().SingleOrDefault().NameOrDefault();

    public static IFootprint GetFootprint(this IGameObjectBlueprint blueprint) =>
        blueprint.GetComponents().OfType<IObjectFootprint>().SingleOrDefault()?.Footprint ?? Footprint.Single;

    public static ResourceAmount GetResourceCost(this IGameObjectBlueprint blueprint) =>
        blueprint.GetComponents().OfType<ICost>().SingleOrDefault()?.Resources ?? ResourceAmount.Zero;

    public static IEnumerable<IBuildBuildingPrecondition> GetBuildBuildingPreconditions(
        this IGameObjectBlueprint blueprint) =>
        blueprint.GetComponents().OfType<IBuildBuildingPrecondition>();
}
