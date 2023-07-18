using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.Utilities.Linq;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.GameLoop;

sealed partial class WaveGenerator
{
    private const int minGroupSize = 7;
    private const int maxGroupSize = minGroupSize * 3;
    private const double targetDensity = 0.6;

    private EnemySpawnScript generateSpawnScript(
        RoutineComposition composition, IEnumerable<SpawnLocation> spawnLocations, Random random)
    {
        var spawnLocationArray = spawnLocations.ToImmutableArray();
        var spawnLocationCount = spawnLocationArray.Length;
        if (spawnLocationCount == 0)
        {
            throw new InvalidOperationException("Routine must be assigned at least one spawn location.");
        }

        var allBatchesPerLocation = composition.Batches
            .SelectMany(comp => generateBatchesPerLocation(comp, spawnLocationCount, random))
            .ToImmutableArray();
        var routineDuration = determineRoutineDuration(allBatchesPerLocation);
        var spawnTimes = new EnemySpawnTimes(spawnEventsBatchedInDuration(allBatchesPerLocation, routineDuration));
        return new EnemySpawnScript(
            spawnTimes.SpawnTimes.SelectMany(_ => spawnLocationArray,
                    (time, location) => new EnemySpawnScript.EnemySpawnEvent(location, time.TimeOffset, time.EnemyForm))
                .ToImmutableArray());
    }

    private ImmutableArray<BatchSequence> generateBatchesPerLocation(
        BatchComposition batchComposition, int spawnLocationCount, Random random)
    {
        var countsPerLocation = batchComposition.Forms.ToImmutableDictionary(
            form => form.EnemyForm,
            form => Math.Max(1, form.SpawnCount / spawnLocationCount));
        // TODO: optimize counts (do we want to optimize on the routine level? may not be necessary)
        var totalSpawnCount = batchComposition.Forms.Sum(form => form.SpawnCount);
        var batchCount = determineBatchCount(totalSpawnCount, random);
        return generateBatchSequences(countsPerLocation, batchCount, random);
    }

    private static int determineBatchCount(int enemyCount, Random random)
    {
        var minGroupsNeeded = enemyCount / maxGroupSize + (enemyCount % maxGroupSize == 0 ? 0 : 1);
        var maxGroups = enemyCount / minGroupSize;

        return minGroupsNeeded >= maxGroups ? minGroupsNeeded : random.Next(minGroupsNeeded, maxGroups + 1);
    }

    private static ImmutableArray<BatchSequence> generateBatchSequences(
        ImmutableDictionary<EnemyForm, int> formCounts, int batchCount, Random random)
    {
        var builder = new List<EnemyForm>[batchCount];

        for (var i = 0; i < batchCount; i++)
        {
            builder[i] = new List<EnemyForm>();
        }

        var n = 0;
        foreach (var (form, count) in formCounts)
        {
            for (var i = 0; i < count; i++, n++)
            {
                builder[n % batchCount].Add(form);
            }
        }

        return builder.Select(list => generateBatchSequence(list, random)).Shuffled(random).ToImmutableArray();
    }

    private static BatchSequence generateBatchSequence(IEnumerable<EnemyForm> forms, Random random)
    {
        var immutableForms = forms.Shuffled(random).ToImmutableArray();
        return new BatchSequence(immutableForms);
    }

    private record struct BatchSequence(ImmutableArray<EnemyForm> Forms);

    private static TimeSpan determineRoutineDuration(IEnumerable<BatchSequence> batchSequences)
    {
        var spawnCount = batchSequences.Sum(s => s.Forms.Length);
        return TimeSpan.Max(
            TimeSpan.Min(
                Constants.Game.WaveGeneration.EnemyTrainLength,
                Constants.Game.WaveGeneration.MaxTimeBetweenSpawns * (spawnCount - 1)),
            Constants.Game.WaveGeneration.MinTimeBetweenSpawns * (spawnCount - 1));
    }

    private static ImmutableArray<EnemySpawnTimes.EnemySpawnTime> spawnEventsBatchedInDuration(
        IList<BatchSequence> batchSequences, TimeSpan duration)
    {
        switch (batchSequences.Count)
        {
            case < 1:
                throw new InvalidOperationException();
            case 1:
                return spawnEventsSpreadUniformlyInDuration(batchSequences[0].Forms, TimeSpan.Zero, duration)
                    .ToImmutableArray();
        }

        var totalEnemies = batchSequences.Sum(s => s.Forms.Length);

        var totalTimeForEnemies = duration * targetDensity;
        var timePerEnemy = totalTimeForEnemies / (totalEnemies - batchSequences.Count);
        var individualBreakDuration = duration * (1 - targetDensity) / (batchSequences.Count - 1);
        var result = ImmutableArray.CreateBuilder<EnemySpawnTimes.EnemySpawnTime>(totalEnemies);
        var idx = 0;

        for (var i = 0; i < batchSequences.Count; i++)
        {
            var batchOffset = i == 0 ? TimeSpan.Zero : result[idx - 1].TimeOffset + individualBreakDuration;
            var batchDuration = timePerEnemy * (batchSequences[i].Forms.Length - 1);

            foreach (var spawnEvent in spawnEventsSpreadUniformlyInDuration(
                         batchSequences[i].Forms, batchOffset, batchDuration))
            {
                result.Add(spawnEvent);
                idx++;
            }
        }

        return result.MoveToImmutable();
    }

    private static IEnumerable<EnemySpawnTimes.EnemySpawnTime> spawnEventsSpreadUniformlyInDuration(
        IReadOnlyList<EnemyForm> forms,
        TimeSpan additionalOffset,
        TimeSpan duration)
    {
        return offsetsSpreadUniformlyInDuration(forms.Count, additionalOffset, duration)
            .Select((offset, i) => new EnemySpawnTimes.EnemySpawnTime(offset, forms[i]));
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
