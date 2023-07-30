using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Enemies;
using static Bearded.TD.Game.GameLoop.WaveTemplate;
using static Bearded.TD.Game.GameLoop.WaveTemplate.ElementSelection;
using static Bearded.TD.Game.Simulation.Enemies.Archetype;

namespace Bearded.TD.Game.GameLoop;

static class WaveTemplates
{
    // Purposefully easier
    public static readonly ScriptTemplate SingleElite = script(
        routine(0.9,
            batch(1,
                form(1, PrimaryElement, Elite))));

    public static readonly ScriptTemplate EliteMinionMix = script(
        routine(1,
            batch(0.75,
                form(1, PrimaryElement, Elite)),
            batch(0.25,
                form(1, PrimaryElement, Minion))));

    // Purposefully harder
    public static readonly ScriptTemplate TwoEliteRoutines = script(
        routine(0.6,
            batch(1,
                form(1, PrimaryElement, Elite))),
        routine(0.6,
            batch(1,
                form(1, PrimaryElement, Elite))));

    public static readonly ScriptTemplate PrimaryEliteAccentMinionMix = script(
        routine(1,
            batch(1,
                form(0.75, PrimaryElement, Elite),
                form(0.25, AccentElement, Minion))));

    // Purposefully harder
    public static readonly ScriptTemplate PrimaryEliteMinionMixWithAccentEliteBatch = script(
        routine(1,
            batch(0.8,
                form(0.75, PrimaryElement, Elite),
                form(0.25, PrimaryElement, Minion)),
            batch(0.4,
                form(1, AccentElement, Elite))));

    public static readonly ImmutableArray<ScriptTemplate> Chapter = ImmutableArray.Create(
        SingleElite,
        EliteMinionMix,
        TwoEliteRoutines,
        PrimaryEliteAccentMinionMix,
        PrimaryEliteMinionMixWithAccentEliteBatch
    );

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
