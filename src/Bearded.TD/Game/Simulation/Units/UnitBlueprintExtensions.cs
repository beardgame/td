using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Units;

static class UnitBlueprintExtensions
{
    public static float GetThreat(this IGameObjectBlueprint blueprint) =>
        blueprint.GetComponents().OfType<IThreat>().SingleOrDefault()?.ThreatCost ?? 0f;
}
