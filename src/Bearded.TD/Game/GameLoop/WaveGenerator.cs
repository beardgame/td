using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using static Bearded.TD.Constants.Game.WaveGeneration;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.GameLoop;

sealed partial class WaveGenerator
{
    private readonly ImmutableArray<ISpawnableEnemy> spawnableEnemies;
    private readonly Faction targetFaction;
    private readonly int seed;
    private readonly Logger logger;
    private readonly EnemyFormGenerator enemyFormGenerator;

    public WaveGenerator(
        ImmutableArray<ISpawnableEnemy> spawnableEnemies,
        IEnumerable<IModule> modules,
        Faction targetFaction,
        int seed,
        Logger logger)
    {
        this.spawnableEnemies = spawnableEnemies;
        this.targetFaction = targetFaction;
        this.logger = logger;
        this.seed = seed;
        enemyFormGenerator = new EnemyFormGenerator(modules, logger);
    }

    public WaveScript GenerateWave(
        WaveRequirements requirements,
        IEnumerable<SpawnLocation> availableSpawnLocations)
    {
        // Ensure that a change in requirements always leads to a (very) different outcome.
        var random = new Random(seed ^ requirements.GetHashCode());
        var (enemyForm, enemiesPerSpawn, spawnLocations, spawnDuration) =
            generateWaveParameters(requirements, availableSpawnLocations, random);

        var enemyScript = toEnemyScript(enemiesPerSpawn, spawnDuration, enemyForm, random);

        return new WaveScript(
            $"Ch {requirements.ChapterNumber}; Wave {requirements.WaveNumber}",
            targetFaction,
            requirements.DowntimeDuration,
            spawnDuration,
            spawnLocations,
            enemyScript);
    }

    private WaveParameters generateWaveParameters(
        WaveRequirements requirements, IEnumerable<SpawnLocation> availableSpawnLocations, Random random)
    {
        logger.Debug?.Log($"Wave parameters requested with threat {requirements.EnemyComposition.TotalThreat}");
        var sw = Stopwatch.StartNew();

        var (minWaveValue, maxWaveValue) = totalThreatRange(requirements);
        var element = requirements.EnemyComposition.Elements.PrimaryElement;

        var (blueprint, blueprintThreat, enemyCount) = chooseEnemy(element, minWaveValue, maxWaveValue, random);

        var spawnLocations = chooseSpawnLocations(enemyCount, availableSpawnLocations, random);
        var spawnLocationCount = spawnLocations.Length;

        var enemiesPerSpawn = MoreMath.CeilToInt((double) enemyCount / spawnLocationCount);

        optimizeForRequestedThreat(requirements, spawnLocationCount, blueprintThreat, ref enemiesPerSpawn);

        var spawnDuration = TimeSpan.Max(
            TimeSpan.Min(
                EnemyTrainLength,
                MaxTimeBetweenSpawns * (enemiesPerSpawn - 1)),
            MinTimeBetweenSpawns * (enemiesPerSpawn - 1));

        logger.Debug?.Log(
            $"Generated wave parameters with {enemiesPerSpawn} enemies of threat {blueprintThreat} at " +
            $"{spawnLocations.Length} spawn locations for a total of " +
            $"{enemiesPerSpawn * spawnLocations.Length * blueprintThreat} threat in {sw.Elapsed.TotalSeconds}s");

        return new WaveParameters(blueprint, enemiesPerSpawn, spawnLocations, spawnDuration);
    }

    private record struct WaveParameters(
        EnemyForm EnemyForm,
        int EnemiesPerSpawn,
        ImmutableArray<SpawnLocation> SpawnLocations,
        TimeSpan SpawnDuration);

    private static (double minThreat, double maxThreat) totalThreatRange(WaveRequirements requirements)
    {
        var requestedThreat = requirements.EnemyComposition.TotalThreat;
        var allowedValueError = requestedThreat * WaveValueErrorFactor;
        var minWaveValue = requestedThreat - allowedValueError;
        var maxWaveValue = requestedThreat + allowedValueError;
        return (minWaveValue, maxWaveValue);
    }

    private static void optimizeForRequestedThreat(
        WaveRequirements requirements, int spawnLocationCount, float blueprintThreat, ref int enemiesPerSpawn)
    {
        var requestedThreat = requirements.EnemyComposition.TotalThreat;

        var actualValue = enemiesPerSpawn * spawnLocationCount * blueprintThreat;
        var actualError = Math.Abs(actualValue - requestedThreat);

        // Try spawning one less enemy per spawn. If that ends up being closer to our desired value, do that.
        var candidateValue = (enemiesPerSpawn - 1) * spawnLocationCount * blueprintThreat;
        var candidateError = Math.Abs(candidateValue - requestedThreat);
        if (candidateValue > 0 && candidateError < actualError)
        {
            enemiesPerSpawn--;
        }
    }
}
