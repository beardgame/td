using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Game.GameLoop;

sealed partial class WaveScheduler
{
    private ImmutableArray<SpawnLocation> chooseSpawnLocations(int enemyCount)
    {
        var activeSpawnLocations = game.Enumerate<SpawnLocation>().Where(s => s.IsAwake).ToList();
        DebugAssert.State.Satisfies(activeSpawnLocations.Count > 0);

        var minSequentialSpawnTime = enemyCount * Constants.Game.WaveGeneration.MinTimeBetweenSpawns;
        var minSpawnPoints =
            MoreMath.CeilToInt(Constants.Game.WaveGeneration.EnemyTrainLength / (minSequentialSpawnTime * enemyCount));
        var numSpawnPoints = activeSpawnLocations.Count <= minSpawnPoints
            ? minSpawnPoints
            : random.Next(minSpawnPoints, activeSpawnLocations.Count);
        if (numSpawnPoints >= enemyCount / 3)
        {
            numSpawnPoints = Math.Max(1, enemyCount / 3);
        }

        var spawnLocations = activeSpawnLocations.RandomSubset(numSpawnPoints, random).ToImmutableArray();
        return spawnLocations;
    }
}
