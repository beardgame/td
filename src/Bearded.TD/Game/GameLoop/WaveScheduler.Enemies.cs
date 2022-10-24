using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.GameLoop;

sealed partial class WaveScheduler
{
    // The minimum number of enemies the wave should support to allow an enemy to be used for the current wave.
    private const int minEnemiesCount = 12;

    private EnemiesToSpawn chooseEnemy(
        double maxWaveValue, double minWaveValue)
    {
        var eligibleEnemies = filteredEligibleEnemies(maxWaveValue);
        var blueprint = selectBlueprint(eligibleEnemies);
        var blueprintThreat = blueprint.GetThreat();

        var (minEnemies, maxEnemies) = enemyCountRange(minWaveValue, blueprintThreat, maxWaveValue);
        var enemyCount = maxEnemies <= minEnemies ? minEnemies : random.Next(minEnemies, maxEnemies + 1);
        return new EnemiesToSpawn(blueprint, blueprintThreat, enemyCount);
    }

    private ImmutableArray<ISpawnableEnemy> filteredEligibleEnemies(double maxWaveValue)
    {
        var eligibleEnemies = spawnableEnemies.Where(spawnableEnemy =>
        {
            var threat = spawnableEnemy.Blueprint.GetThreat();
            return minEnemiesCount * threat < maxWaveValue;
        }).ToImmutableArray();
        if (eligibleEnemies.Length == 0)
        {
            throw new InvalidOperationException();
        }

        return eligibleEnemies;
    }

    private IGameObjectBlueprint selectBlueprint(IReadOnlyList<ISpawnableEnemy> enemies)
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

    private static (int minEnemies, int maxEnemies) enemyCountRange(
        double minWaveValue, float blueprintThreat, double maxWaveValue)
    {
        var minEnemies = MoreMath.CeilToInt(minWaveValue / blueprintThreat);
        var maxEnemies = MoreMath.FloorToInt(maxWaveValue / blueprintThreat);
        return (minEnemies, maxEnemies);
    }

    [UsedImplicitly] // type is deconstructed
    private record struct EnemiesToSpawn(IGameObjectBlueprint Blueprint, float Threat, int SpawnCount);
}
