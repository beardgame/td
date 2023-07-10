using System;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.Model;

namespace Bearded.TD.Game.GameLoop;

// TODO: storing the threat percentages in the actual records means we can't reuse those records elsewhere. We may for
//       example want to have standardized routines or batches that we can insert in places. We may in turn even allow
//       the elements as inputs for nested structured (e.g. we may generate a routine with primary and accent elements
//       reversed. For now, YAGNI and we'll stick with just storing the templates as single-use objects.
static class WaveTemplate
{
    public sealed record ScriptTemplate(ImmutableArray<RoutineTemplate> Routines);

    public sealed record RoutineTemplate(ImmutableArray<BatchTemplate> Batches, double ThreatPercentage);

    public sealed record BatchTemplate(ImmutableArray<FormTemplate> Forms, double ThreatPercentage);

    public sealed record FormTemplate(ElementSelection Element, Archetype Archetype, double ThreatPercentage);

    public enum ElementSelection
    {
        PrimaryElement,
        AccentElement,
    }

    public static Element ChooseElement(this ElementSelection selection, ElementalTheme theme) =>
        selection switch
        {
            ElementSelection.PrimaryElement => theme.PrimaryElement,
            ElementSelection.AccentElement => theme.AccentElement,
            _ => throw new ArgumentOutOfRangeException(nameof(selection), selection, null)
        };
}
