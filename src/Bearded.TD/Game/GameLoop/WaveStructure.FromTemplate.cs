using System.Collections.Immutable;
using System.Linq;
using static Bearded.TD.Game.GameLoop.WaveTemplate;

namespace Bearded.TD.Game.GameLoop;

static partial class WaveStructure
{
    public static ScriptStructure FromTemplate(ScriptTemplate template, WaveEnemyComposition requirements)
    {
        var myThreat = requirements.TotalThreat;
        var elements = requirements.Elements;
        return new ScriptStructure(
            template.Routines.Select(t => fromTemplate(t, myThreat, elements)).ToImmutableArray());
    }

    private static RoutineStructure fromTemplate(RoutineTemplate template, double parentThreat, ElementalTheme elements)
    {
        var myThreat = parentThreat * template.ThreatPercentage;
        return new RoutineStructure(
            template.Batches.Select(t => fromTemplate(t, myThreat, elements)).ToImmutableArray());
    }

    private static BatchStructure fromTemplate(
        BatchTemplate template, double parentThreat, ElementalTheme elements)
    {
        var myThreat = parentThreat * template.ThreatPercentage;
        return new BatchStructure(
            template.Forms.Select(t => fromTemplate(t, myThreat, elements)).ToImmutableArray());
    }

    private static FormStructure fromTemplate(
        FormTemplate template, double parentThreat, ElementalTheme elements) =>
        new(
            template.Element.ChooseElement(elements),
            template.Archetype,
            parentThreat * template.ThreatPercentage);
}
