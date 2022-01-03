using System.Linq;
using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Units;

static class UnitBlueprintExtensions
{
    public static float GetThreat(this IComponentOwnerBlueprint blueprint) =>
        blueprint.GetComponents<ComponentGameObject>().OfType<IThreat>().SingleOrDefault()?.ThreatCost ?? 0f;
}
