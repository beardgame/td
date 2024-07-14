using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using static Bearded.TD.Constants.Game.WaveGeneration;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.GameLoop;

sealed partial class WaveGenerator
{
    private const int minGroupSize = 7;
    private const int maxGroupSize = minGroupSize * 3;
    private const double targetDensity = 0.6;
    private const float maxSpawnPerturbation = 0.2f;
    private static readonly ImmutableDictionary<Archetype, TimeSpan> targetSpawnRates = ImmutableDictionary.CreateRange(
        new Dictionary<Archetype, TimeSpan>
        {
            { Archetype.Minion, 0.2.S() },
            { Archetype.Elite, 1.S() },
            { Archetype.Champion, 2.S() },
            { Archetype.Boss, 4.S() }
        });
    private static readonly ImmutableDictionary<Archetype, TimeSpan> maxSpawnRates = ImmutableDictionary.CreateRange(
        new Dictionary<Archetype, TimeSpan>
        {
            { Archetype.Minion, 0.1.S() },
            { Archetype.Elite, 0.3.S() },
            { Archetype.Champion, 0.5.S() },
            { Archetype.Boss, 0.5.S() }
        });

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
        var spawnTimes = new EnemySpawnTimes(spawnEventsBatchedInDuration(allBatchesPerLocation, random));
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
        return generateBatchSequences(
            batchComposition.Forms.Select(form => form.EnemyForm),
            countsPerLocation,
            batchCount,
            random);
    }

    private static int determineBatchCount(int enemyCount, Random random)
    {
        var minGroupsNeeded = enemyCount / maxGroupSize + (enemyCount % maxGroupSize == 0 ? 0 : 1);
        var maxGroups = enemyCount / minGroupSize;

        return minGroupsNeeded >= maxGroups ? minGroupsNeeded : random.Next(minGroupsNeeded, maxGroups + 1);
    }

    private static ImmutableArray<BatchSequence> generateBatchSequences(
        IEnumerable<EnemyForm> enemyForms,
        IReadOnlyDictionary<EnemyForm, int> countsByForm,
        int batchCount,
        Random random)
    {
        var builder = new List<EnemyForm>[batchCount];

        for (var i = 0; i < batchCount; i++)
        {
            builder[i] = new List<EnemyForm>();
        }

        var n = 0;
        foreach (var form in enemyForms)
        {
            var count = countsByForm.GetValueOrDefault(form, 0);
            for (var i = 0; i < count; i++, n++)
            {
                builder[n % batchCount].Add(form);
            }
        }

        return builder.Select(list => generateBatchSequence(list, random)).Shuffled(random).ToImmutableArray();
    }

    private static BatchSequence generateBatchSequence(IEnumerable<EnemyForm> forms, Random random)
    {
        var immutableForms = forms.Shuffled(random)
            .Select(form => new EnemyFormWithArchetype(form, form.Blueprint.GetArchetype()))
            .ToImmutableArray();
        return new BatchSequence(immutableForms);
    }

    private record struct BatchSequence(ImmutableArray<EnemyFormWithArchetype> Forms);

    // We access the archetype more than once, so we cache it.
    private record struct EnemyFormWithArchetype(EnemyForm Form, Archetype Archetype);

    private static ImmutableArray<EnemySpawnTimes.EnemySpawnTime> spawnEventsBatchedInDuration(
        IList<BatchSequence> batchSequences, Random random)
    {
        var enemyCountByArchetype = batchSequences.SelectMany(s => s.Forms)
            .GroupBy(f => f.Archetype)
            .ToImmutableDictionary(group => group.Key, group => group.Count());
        var spawnRates = findOptimizedSpawnRates(enemyCountByArchetype, out var totalSpawnDuration);
        var totalTimeBetweenBatches = (1 - targetDensity) / targetDensity * totalSpawnDuration;
        var timeBetweenBatchPairs = batchSequences.Count <= 1
            ? TimeSpan.Zero
            : totalTimeBetweenBatches / (batchSequences.Count - 1);

        var result = ImmutableArray.CreateBuilder<EnemySpawnTimes.EnemySpawnTime>();
        var offset = TimeSpan.Zero;

        foreach (var batchSequence in batchSequences)
        {
            foreach (var (form, archetype) in batchSequence.Forms)
            {
                var spawnRate = spawnRates[archetype];
                var offsetWithinWindow =
                    (0.5f + random.NextFloat(-maxSpawnPerturbation, maxSpawnPerturbation)) * spawnRate;
                result.Add(new EnemySpawnTimes.EnemySpawnTime(offset + offsetWithinWindow, form));
                offset += spawnRate;
            }

            offset += timeBetweenBatchPairs;
        }

        return result.ToImmutable();
    }

    private static ImmutableDictionary<Archetype, TimeSpan> findOptimizedSpawnRates(
        IDictionary<Archetype, int> enemyCountByArchetype, out TimeSpan totalSpawnDuration)
    {
        var totalSpawnDurationWithoutAdjustments = spawnDuration(enemyCountByArchetype, targetSpawnRates);
        if (totalSpawnDurationWithoutAdjustments <= TargetSpawnDuration)
        {
            totalSpawnDuration = totalSpawnDurationWithoutAdjustments;
            return targetSpawnRates;
        }

        var spawnRates = new Dictionary<Archetype, TimeSpan>(targetSpawnRates);
        var compressibleDuration = totalSpawnDurationWithoutAdjustments;
        var compressibleArchetypes = new List<Archetype>(enemyCountByArchetype.Keys);

        while (compressibleArchetypes.Count > 0)
        {
            var leastCompressibleArchetype =
                Enumerable.MinBy(compressibleArchetypes, a => spawnRates[a] / maxSpawnRates[a]);

            var targetCompressionFactor = compressibleDuration / TargetSpawnDuration;
            var availableCompressionFactor =
                spawnRates[leastCompressibleArchetype] / maxSpawnRates[leastCompressibleArchetype];

            if (targetCompressionFactor <= availableCompressionFactor)
            {
                foreach (var a in compressibleArchetypes)
                {
                    spawnRates[a] /= targetCompressionFactor;
                }
                break;
            }

            foreach (var a in compressibleArchetypes)
            {
                spawnRates[a] /= availableCompressionFactor;
            }

            compressibleArchetypes.Remove(leastCompressibleArchetype);
            compressibleDuration = compressibleArchetypes.Count == 0
                ? TimeSpan.Zero
                : compressibleArchetypes
                    .Select(a => spawnRates[a] * enemyCountByArchetype[a])
                    .Aggregate((l, r) => l + r);
        }

        totalSpawnDuration = spawnDuration(enemyCountByArchetype, spawnRates);
        return ImmutableDictionary.CreateRange(spawnRates);
    }

    private static TimeSpan spawnDuration(
        IDictionary<Archetype, int> enemyCountByArchetype, IDictionary<Archetype, TimeSpan> spawnRates)
    {
        return enemyCountByArchetype.Select(kvp => spawnRates[kvp.Key] * kvp.Value).Aggregate((l, r) => l + r);
    }
}
