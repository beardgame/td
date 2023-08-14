using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Game.Generation.Semantic.Props;

/*
 * WIP notes:
 *
 * - Post processors are now a behaviour, but what are they actually a behaviour on? We don't have a "map" object, so
 *   they can't easily be loaded as a list of behaviours for an existing object. Game modes would be awkward, since all
 *   the level generation stuff is handled by rules right now. Should they be behaviours at all?
 *
 * - The current implementation assumes the following steps in the algorithm (though not all steps are fully supported
 *   in MVP):
 *   1. loop through all post processors and let them register all the props they can spawn;
 *   2. somehow sort the options so that we have the most restrictive options first;
 *     - down the line, props may also include some kind of "affinity" score, how badly they want to be in that
 *       particular spot, based on biome, wetness, temperature, etc.
 *   3. choose props to spawn, giving preference to the more restrictive options.
 *   The reason this approach was chosen was to allow post-processors fulfilling multiple hints at once. For example, a
 *   postprocessor might detect three blockers in a triangle, and put one big rock on them, rather than always having to
 *   stick with single tile props.
 *   Even props that don't follow a hint may use a similar collision avoidance system, to ensure we don't spawn props on
 *   top of each other randomly.
 *   The current implementation isn't this smart yet, but we still need to first register options and then pick one to
 *   ensure the first post processor doesn't just fulfil all hints. This leads to follow-up questions though:
 *
 *   - What about more malleable features? A patch of moss could easily lose a tile and still work. It shouldn't block
 *     other things, or be blocked as easily by other things. <- don't worry about
 *   - Do we really need to go this flexible? Can we instead just let the mod writer divide the space of prop hints in
 *     a disjoint covering set of the entire space? That sounds like a big hassle, and may get complicated if we start
 *     working with more freeform tags. <- no
 *
 * - Replace blueprint with list of blueprints
 */

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
