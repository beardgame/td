using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Game.Generation.Semantic.Props;

delegate void SolutionAction(PropContentGenerationContext context);

sealed class PropSolver
{
    private readonly Logger logger;
    private readonly List<PropSolution> solutions = new();

    public PropSolver(Logger logger)
    {
        this.logger = logger;
    }

    public void AddOption(PropHint handledHint, SolutionAction solutionAction)
    {
        solutions.Add(new PropSolution(handledHint, solutionAction));
    }

    public void CommitSolutions(IEnumerable<PropHint> hints, PropContentGenerationContext context, Random random)
    {
        solutions.Shuffle(random);
        var allHints = hints.ToImmutableHashSet();
        var completedHints = new HashSet<PropHint>();

        foreach (var solution in solutions)
        {
            if (!allHints.Contains(solution.HandledHint) || completedHints.Contains(solution.HandledHint))
            {
                continue;
            }

            solution.SolutionAction(context);
            completedHints.Add(solution.HandledHint);
        }

        var unhandledHints = allHints.Except(completedHints);
        if (!unhandledHints.IsEmpty)
        {
            logUnhandledHints(unhandledHints);
        }
    }

    private void logUnhandledHints(ImmutableHashSet<PropHint> unhandledHints)
    {
        logger.Warning?.Log("Some hints did not have solutions offered:");
        foreach (var hint in unhandledHints)
        {
            logger.Warning?.Log($"  - {hint}");
        }
    }

    private readonly record struct PropSolution(PropHint HandledHint, SolutionAction SolutionAction);
}
