using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.Units;
using Bearded.Utilities;
using JetBrains.Annotations;
using static Bearded.TD.Constants.Game.WaveGeneration;
using static Bearded.TD.Game.GameLoop.WaveStructure;

namespace Bearded.TD.Game.GameLoop;

sealed partial class WaveGenerator
{
    [UsedImplicitly] // honestly not sure why, but Rider isn't picking up the access, even though there are usages
    private record struct RoutineComposition(ImmutableArray<BatchComposition> Batches, int RequestedSpawnLocationCount);

    private record struct BatchComposition(ImmutableArray<GeneratedForm> Forms);

    [UsedImplicitly] // type is deconstructed
    private record struct GeneratedForm(EnemyForm EnemyForm, float Threat, int SpawnCount);

    private RoutineComposition generateRoutineComposition(RoutineStructure structure, Random random)
    {
        var batchCompositions =
            structure.Batches.Select(batch => generateBatchComposition(batch, random)).ToImmutableArray();

        // TODO: be more sophisticated about how many spawn locations we want, consider archetypes separately too
        var totalEnemyCount = batchCompositions.SelectMany(comp => comp.Forms).Sum(form => form.SpawnCount);
        var requestedSpawnCount = 1 + (totalEnemyCount / 30);

        return new RoutineComposition(batchCompositions, requestedSpawnCount);
    }

    private BatchComposition generateBatchComposition(BatchStructure structure, Random random)
    {
        var generatedForms = structure.Forms.Select(form => generateForm(form, random)).ToImmutableArray();
        return new BatchComposition(generatedForms);
    }

    private GeneratedForm generateForm(FormStructure structure, Random random)
    {
        var enemyForm = chooseEnemy(structure, random);
        var blueprintThreat = enemyForm.Blueprint.GetThreat();
        var enemyCount = chooseSpawnCount(blueprintThreat, structure.TotalThreat, random);
        return new GeneratedForm(enemyForm, blueprintThreat, enemyCount);
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
}
