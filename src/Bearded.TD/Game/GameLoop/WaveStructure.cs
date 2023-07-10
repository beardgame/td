using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.Model;

namespace Bearded.TD.Game.GameLoop;

static partial class WaveStructure
{
    public sealed record ScriptStructure(ImmutableArray<RoutineStructure> Routines);

    public sealed record RoutineStructure(ImmutableArray<BatchStructure> Batches);

    public sealed record BatchStructure(ImmutableArray<FormStructure> Forms);

    public sealed record FormStructure(Element Element, Archetype Archetype, double TotalThreat);
}
