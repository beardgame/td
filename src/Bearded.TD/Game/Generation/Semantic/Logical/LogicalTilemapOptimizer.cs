using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Fitness;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Game.Generation.Semantic.Logical;

sealed class LogicalTilemapOptimizer
{
    private readonly Logger logger;
    private readonly ImmutableArray<ILogicalTilemapMutation> mutations;
    private readonly FitnessFunction<LogicalTilemap> fitnessFunction;

    public LogicalTilemapOptimizer(
        Logger logger,
        ImmutableArray<ILogicalTilemapMutation> mutations,
        FitnessFunction<LogicalTilemap> fitnessFunction)
    {
        this.logger = logger;
        this.mutations = mutations;
        this.fitnessFunction = fitnessFunction;
    }

    public LogicalTilemap Optimize(LogicalTilemap logicalTilemap, Random random)
    {
        logger.Debug?.Log("Optimizing logical tilemap");
        var currentBest = logicalTilemap;
        var currentFitness = fitnessFunction.FitnessOf(currentBest);

        logger.Trace?.Log($"Initial fitness\n{currentFitness}");

        var generatedCount = 500;
        var mutationsPerGeneration = 5;
        var acceptedCount = 0;
        var lastImprovement = -1;

        foreach (var i in Enumerable.Range(0, generatedCount))
        {
            var mutated = mutate(currentBest, mutationsPerGeneration, random);
            var mutatedFitness = fitnessFunction.FitnessOf(mutated);

            if (mutatedFitness.Value < currentFitness.Value)
            {
                currentBest = mutated;
                currentFitness = mutatedFitness;
                acceptedCount++;
                lastImprovement = i;
            }
        }

        logger.Trace?.Log($"Final fitness:\n{currentFitness}");
        logger.Trace?.Log($"Accepted {acceptedCount} mutations, last was {lastImprovement}/{generatedCount}");

        return currentBest;
    }

    private LogicalTilemap mutate(LogicalTilemap currentBest, int numMutations, Random random)
    {
        var copy = currentBest.Clone();

        var mutationsMade = 0;
        while (mutationsMade < numMutations)
        {
            var mutation = mutations.RandomElement(random);
            if (mutation.TryMutate(copy, random))
            {
                mutationsMade++;
            }
        }

        return copy;
    }
}