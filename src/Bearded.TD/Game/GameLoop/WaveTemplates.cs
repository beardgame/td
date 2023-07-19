using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Enemies;
using static Bearded.TD.Game.GameLoop.WaveTemplate;
using static Bearded.TD.Game.GameLoop.WaveTemplate.ElementSelection;
using static Bearded.TD.Game.Simulation.Enemies.Archetype;

namespace Bearded.TD.Game.GameLoop;

static class WaveTemplates
{
    public static readonly ScriptTemplate Legacy = script(
        routine(1,
            batch(1,
                form(1, PrimaryElement, Elite))));

    private static ScriptTemplate script(params RoutineTemplate[] routines)
    {
        return new ScriptTemplate(routines.ToImmutableArray());
    }

    private static RoutineTemplate routine(double threatPercentage, params BatchTemplate[] batches)
    {
        return new RoutineTemplate(batches.ToImmutableArray(), threatPercentage);
    }

    private static BatchTemplate batch(double threatPercentage, params FormTemplate[] forms)
    {
        return new BatchTemplate(forms.ToImmutableArray(), threatPercentage);
    }

    private static FormTemplate form(double threatPercentage, ElementSelection element, Archetype archetype)
    {
        return new FormTemplate(element, archetype, threatPercentage);
    }
}
