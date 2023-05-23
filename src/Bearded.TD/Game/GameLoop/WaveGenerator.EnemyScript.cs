using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.GameLoop;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.GameLoop;

sealed partial class WaveGenerator
{
    private const int minGroupSize = 7;
    private const int maxGroupSize = minGroupSize * 3;
    private const double targetDensity = 0.6;

    private EnemySpawnScript toEnemyScript(
        int enemyCount,
        TimeSpan waveDuration,
        EnemyForm enemyForm,
        IEnumerable<SpawnLocation> spawnLocations,
        Random random)
    {
        var enumeratedSpawnLocations = spawnLocations.ToImmutableArray();
        var times = toEnemyTimes(enemyCount, waveDuration, enemyForm, random);
        return new EnemySpawnScript(
            times.SpawnTimes.SelectMany(_ => enumeratedSpawnLocations,
                    (time, location) => new EnemySpawnScript.EnemySpawnEvent(location, time.TimeOffset, time.EnemyForm))
                .ToImmutableArray());
    }

    private EnemySpawnTimes toEnemyTimes(
        int enemyCount,
        TimeSpan waveDuration,
        EnemyForm enemyForm,
        Random random)
    {
        var groupCount = enemyCount < minGroupSize * 2 ? 1 : determineGroupCount(enemyCount, random);
        if (groupCount == 1)
        {
            return new EnemySpawnTimes(
                spawnEventsSpreadUniformlyInDuration(enemyCount, TimeSpan.Zero, waveDuration, enemyForm)
                    .ToImmutableArray());
        }

        var batchSizes = determineBatchSizes(enemyCount, groupCount);

        return new EnemySpawnTimes(
            spawnEventsBatchedInDuration(batchSizes, waveDuration, enemyForm).ToImmutableArray());
    }

    private int determineGroupCount(int enemyCount, Random random)
    {
        var minGroupsNeeded = enemyCount / maxGroupSize + (enemyCount % maxGroupSize == 0 ? 0 : 1);
        var maxGroups = enemyCount / minGroupSize;

        return minGroupsNeeded >= maxGroups ? minGroupsNeeded : random.Next(minGroupsNeeded, maxGroups + 1);
    }

    private static ImmutableArray<int> determineBatchSizes(int enemyCount, int groupCount)
    {
        var enemiesPerGroup = enemyCount / groupCount;
        var enemiesLeft = enemyCount % groupCount;

        return Enumerable.Repeat(enemiesPerGroup + 1, enemiesLeft)
            .Concat(Enumerable.Repeat(enemiesPerGroup, groupCount - enemiesLeft))
            .ToImmutableArray();
    }

    private ImmutableArray<EnemySpawnTimes.EnemySpawnTime> spawnEventsBatchedInDuration(
        IList<int> batchSizes, TimeSpan duration, EnemyForm enemyForm)
    {
        if (batchSizes.Count < 2)
        {
            throw new InvalidOperationException();
        }
        var totalEnemies = batchSizes.Sum();

        var totalTimeForEnemies = duration * targetDensity;
        var timePerEnemy = totalTimeForEnemies / (totalEnemies - batchSizes.Count);
        var individualBreakDuration = duration * (1 - targetDensity) / (batchSizes.Count - 1);
        var result = ImmutableArray.CreateBuilder<EnemySpawnTimes.EnemySpawnTime>(totalEnemies);
        var idx = 0;

        for (var i = 0; i < batchSizes.Count; i++)
        {
            var batchOffset = i == 0 ? TimeSpan.Zero : result[idx - 1].TimeOffset + individualBreakDuration;
            var batchDuration = timePerEnemy * (batchSizes[i] - 1);

            foreach (var spawnEvent in spawnEventsSpreadUniformlyInDuration(
                         batchSizes[i], batchOffset, batchDuration, enemyForm))
            {
                result.Add(spawnEvent);
                idx++;
            }
        }

        return result.MoveToImmutable();
    }

    private static IEnumerable<EnemySpawnTimes.EnemySpawnTime> spawnEventsSpreadUniformlyInDuration(
        int count,
        TimeSpan additionalOffset,
        TimeSpan duration,
        EnemyForm enemyForm)
    {
        return offsetsSpreadUniformlyInDuration(count, additionalOffset, duration)
            .Select(offset => new EnemySpawnTimes.EnemySpawnTime(offset, enemyForm));
    }

    private static IEnumerable<TimeSpan> offsetsSpreadUniformlyInDuration(
        int count, TimeSpan additionalOffset, TimeSpan duration)
    {
        switch (count)
        {
            case < 1:
                throw new ArgumentException("Count must be positive", nameof(count));
            case 1:
                return ImmutableArray.Create(additionalOffset);
            default:
            {
                var timeBetweenOffsets = duration / (count - 1);
                return Enumerable.Range(0, count).Select(i => additionalOffset + i * timeBetweenOffsets);
            }
        }
    }
}
