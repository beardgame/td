using System;
using System.Linq;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Units;

static class UnitBlueprintExtensions
{
    public static float GetThreat(this IGameObjectBlueprint blueprint) =>
        blueprint.GetComponents().OfType<IThreat>().SingleOrDefault()?.ThreatCost ?? 0f;

    public static Archetype GetArchetype(this IGameObjectBlueprint blueprint) =>
        blueprint.GetComponents().OfType<IProperty<Archetype>>().SingleOrDefault()?.Value ??
        throw new InvalidOperationException("Attempted to find an archetype but none was found.");
}
