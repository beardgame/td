using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.GameLoop;

sealed partial class WaveGenerator
{
    private ImmutableArray<SpawnLocation> chooseSpawnLocations(
        int enemyCount, IEnumerable<SpawnLocation> availableSpawnLocations, Random random)
    {
        var activeSpawnLocations = availableSpawnLocations.ToImmutableArray();
        State.Satisfies(activeSpawnLocations.Length > 0);

        var minSequentialSpawnTime = enemyCount * Constants.Game.WaveGeneration.MinTimeBetweenSpawns;
        var minSpawnPoints =
            MoreMath.CeilToInt(Constants.Game.WaveGeneration.EnemyTrainLength / (minSequentialSpawnTime * enemyCount));
        var numSpawnPoints = activeSpawnLocations.Length <= minSpawnPoints
            ? minSpawnPoints
            : random.Next(minSpawnPoints, activeSpawnLocations.Length);
        if (numSpawnPoints >= enemyCount / 3)
        {
            numSpawnPoints = Math.Max(1, enemyCount / 3);
        }

        var spawnLocations = activeSpawnLocations.RandomSubset(numSpawnPoints, random).ToImmutableArray();
        return spawnLocations;
    }
}
