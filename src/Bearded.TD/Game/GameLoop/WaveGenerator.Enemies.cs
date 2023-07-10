using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using JetBrains.Annotations;
using static Bearded.TD.Constants.Game.WaveGeneration;
using static Bearded.TD.Game.GameLoop.WaveStructure;

namespace Bearded.TD.Game.GameLoop;

sealed partial class WaveGenerator
{
    private EnemiesToSpawn chooseEnemy(FormStructure structure, Random random)
    {
        var eligibleEnemies = filteredEligibleEnemies(structure);
        var enemyForm = generateFormWithRetries(
            eligibleEnemies, new EnemyFormGenerator.Requirements(structure.Element), 5, random);
        var blueprintThreat = enemyForm.Blueprint.GetThreat();
        var enemyCount = chooseSpawnCount(blueprintThreat, structure.TotalThreat, random);
        return new EnemiesToSpawn(enemyForm, blueprintThreat, enemyCount);
    }

    private ImmutableArray<ISpawnableEnemy> filteredEligibleEnemies(FormStructure structure)
    {
        var maxTotalValue = structure.TotalThreat * (1 + WaveValueErrorFactor);
        var eligibleEnemies = spawnableEnemies.Where(spawnableEnemy =>
        {
            var blueprint = spawnableEnemy.Blueprint;
            var threat = blueprint.GetThreat();
            var archetype = blueprint.GetArchetype();
            var minEnemies = minEnemiesForArchetype(archetype);
            return minEnemies * threat <= maxTotalValue;
        }).ToImmutableArray();
        if (eligibleEnemies.Length == 0)
        {
            throw new InvalidOperationException("Could not find an enemy with a low enough threat to spawn.");
        }

        return eligibleEnemies;
    }

    private static int minEnemiesForArchetype(Archetype archetype) => archetype switch
    {
        Archetype.Minion => 24,
        Archetype.Elite => 4,
        Archetype.Champion => 1,
        Archetype.Boss => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(archetype), archetype, null)
    };

    private EnemyForm generateFormWithRetries(
        ImmutableArray<ISpawnableEnemy> eligibleEnemies,
        EnemyFormGenerator.Requirements requirements,
        int maxRetries,
        Random random)
    {
        for (var i = 0; i < maxRetries; i++)
        {
            var blueprint = selectBlueprint(eligibleEnemies, random);
            if (enemyFormGenerator.TryGenerateEnemyForm(blueprint, requirements, random, out var generatedForm))
            {
                return generatedForm;
            }
        }

        throw new InvalidOperationException(
            $"Could not generate an enemy form with the provided requirements after {maxRetries} tries.");
    }

    private static IGameObjectBlueprint selectBlueprint(IReadOnlyList<ISpawnableEnemy> enemies, Random random)
    {
        var probabilities = new double[enemies.Count + 1];
        foreach (var (enemy, i) in enemies.Indexed())
        {
            probabilities[i + 1] = enemy.Probability + probabilities[i];
        }

        var t = random.NextDouble(probabilities[^1]);
        var result = Array.BinarySearch(probabilities, t);

        var selectedEnemy = result >= 0 ? enemies[result] : enemies[~result - 1];
        return selectedEnemy.Blueprint;
    }

    private static int chooseSpawnCount(double blueprintThreat, double requestedThreat, Random random)
    {
        var (minWaveValue, maxWaveValue) = totalThreatRange(requestedThreat);
        var (minEnemies, maxEnemies) = enemyCountRange(minWaveValue, blueprintThreat, maxWaveValue);
        return maxEnemies <= minEnemies ? minEnemies : random.Next(minEnemies, maxEnemies + 1);
    }

    private static (double minThreat, double maxThreat) totalThreatRange(double requestedThreat)
    {
        var allowedValueError = requestedThreat * WaveValueErrorFactor;
        var minWaveValue = requestedThreat - allowedValueError;
        var maxWaveValue = requestedThreat + allowedValueError;
        return (minWaveValue, maxWaveValue);
    }

    private static (int minEnemies, int maxEnemies) enemyCountRange(
        double minWaveValue, double blueprintThreat, double maxWaveValue)
    {
        var minEnemies = MoreMath.CeilToInt(minWaveValue / blueprintThreat);
        var maxEnemies = MoreMath.FloorToInt(maxWaveValue / blueprintThreat);
        return (minEnemies, maxEnemies);
    }

    [UsedImplicitly] // type is deconstructed
    private record struct EnemiesToSpawn(EnemyForm EnemyForm, float Threat, int SpawnCount);
}
