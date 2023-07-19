using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using static Bearded.TD.Constants.Game.WaveGeneration;
using static Bearded.TD.Game.GameLoop.WaveStructure;

namespace Bearded.TD.Game.GameLoop;

sealed partial class WaveGenerator
{
    private EnemyForm chooseEnemy(FormStructure structure, Random random)
    {
        var eligibleEnemies = filteredEligibleEnemies(structure);
        return generateFormWithRetries(
            eligibleEnemies, new EnemyFormGenerator.Requirements(structure.Element), 5, random);
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
            return archetype == structure.Archetype && minEnemies * threat <= maxTotalValue;
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
        Archetype.Elite => 6,
        Archetype.Champion => 2,
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
}
