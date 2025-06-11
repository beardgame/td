using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using static Bearded.TD.Constants.Game.WaveGeneration;
using static Bearded.TD.Game.GameLoop.WaveStructure;

namespace Bearded.TD.Game.GameLoop;

sealed partial class WaveGenerator
{
    private static readonly EnemyFormGenerator.Requirements fallbackRequirements = new(Element.Kinetics);

    private EnemyForm chooseEnemy(FormStructure structure, Random random)
    {
        // Step 1: filter enemies by threat and archetype to fit the wave structure
        var waveAppropriateEnemies = eligibleEnemiesForStructure(spawnableEnemies, structure);
        if (waveAppropriateEnemies.Length == 0)
        {
            throw new InvalidOperationException(
                "Could not find an enemy with correct archetype or a low enough threat to spawn.");
        }

        // Step 2: filter enemies by the other requirements
        var requirements = new EnemyFormGenerator.Requirements(structure.Element);
        var requirementsAppropriateEnemies = eligibleEnemiesForRequirements(waveAppropriateEnemies, requirements);

        if (requirementsAppropriateEnemies.Length == 0)
        {
            // Fall back to a basic set of requirements that will always match some enemy.
            requirements = fallbackRequirements;
            requirementsAppropriateEnemies =
                eligibleEnemiesForRequirements(waveAppropriateEnemies, requirements);
        }
        if (requirementsAppropriateEnemies.Length == 0)
        {
            // We tried...
            throw new InvalidOperationException(
                "Could not find an enemy which could generate a form that satisfies the requirements.");
        }

        // Step 3: select the enemy blueprint for which we will generate a form - this is where randomness comes in
        var blueprint = selectBlueprint(requirementsAppropriateEnemies, random);

        // Step 4: fill the sockets for the selected blueprint
        return enemyFormGenerator.Generate(blueprint, requirements, random);
    }

    private static ImmutableArray<ISpawnableEnemy> eligibleEnemiesForStructure(
        IEnumerable<ISpawnableEnemy> enemies, FormStructure structure)
    {
        var maxTotalValue = structure.TotalThreat * (1 + WaveValueErrorFactor);
        return enemies.Where(spawnableEnemy =>
        {
            var blueprint = spawnableEnemy.Blueprint;
            var threat = blueprint.GetThreat();
            var archetype = blueprint.GetArchetype();
            var minEnemies = minEnemiesForArchetype(archetype);
            return archetype == structure.Archetype && minEnemies * threat <= maxTotalValue;
        }).ToImmutableArray();
    }

    private static int minEnemiesForArchetype(Archetype archetype) => archetype switch
    {
        Archetype.Minion => 12,
        Archetype.Elite => 6,
        Archetype.Champion => 2,
        Archetype.Boss => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(archetype), archetype, null)
    };

    private ImmutableArray<ISpawnableEnemy> eligibleEnemiesForRequirements(
        IEnumerable<ISpawnableEnemy> enemies, EnemyFormGenerator.Requirements requirements)
    {
        return enemies
            .Where(spawnableEnemy => enemyFormGenerator.CanGenerate(spawnableEnemy.Blueprint, requirements))
            .ToImmutableArray();
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
