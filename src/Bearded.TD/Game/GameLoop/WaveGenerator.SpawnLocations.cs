using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Game.GameLoop;

sealed partial class WaveGenerator
{
    private static ILookup<RoutineComposition, SpawnLocation> assignSpawnLocations(
        IEnumerable<SpawnLocation> spawnLocations, IEnumerable<RoutineComposition> routineCompositions, Random random)
    {
        var shuffledSpawnLocations = spawnLocations.Shuffled(random);
        var routineCompositionsArray = routineCompositions.ToImmutableArray();

        if (routineCompositionsArray.Length > shuffledSpawnLocations.Count)
        {
            throw new InvalidOperationException(
                "Cannot generate a wave with more routines than available spawn locations.");
        }

        // Use a proportional enumerable to assign any spawn locations after assigning each routine the first one based
        // on proportional error.
        var additionalRequestDict =
            routineCompositionsArray.ToImmutableDictionary(comp => comp, comp => comp.RequestedSpawnLocationCount);
        var alreadyAssignedDict =
            routineCompositionsArray.ToImmutableDictionary(comp => comp, _ => 1);
        var requestEnumerable =
            new ProportionalEnumerable<RoutineComposition>(additionalRequestDict, alreadyAssignedDict);

        // We first iterate over the routine compositions once to zip it with the spawn locations, assigning a random
        // spawn location to each. Then we use the proportional enumerable to assign spawn locations to routines until
        // we either run out of routines or spawn locations.
        return routineCompositionsArray
            .Concat(requestEnumerable)
            .Zip(shuffledSpawnLocations)
            .ToLookup(tuple => tuple.First, tuple => tuple.Second);
    }
}
