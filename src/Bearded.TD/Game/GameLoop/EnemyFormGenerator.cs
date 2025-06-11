using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game.GameLoop;

sealed partial class EnemyFormGenerator
{
    private static readonly ImmutableHashSet<Element> allElements = Enum.GetValues<Element>().ToImmutableHashSet();

    private readonly ILookup<SocketShape, IModule> modulesBySocket;
    private readonly Dictionary<IGameObjectBlueprint, PrecalculatedBlueprintSummary> precalculatedSummaries = new();

    private readonly Logger logger;

    public EnemyFormGenerator(IEnumerable<IModule> modules, Logger logger)
    {
        modulesBySocket = modules.ToLookup(m => m.SocketShape);
        this.logger = logger;
    }

    public readonly record struct Requirements(Element AffinityElement);

    private PrecalculatedBlueprintSummary findSummary(IGameObjectBlueprint blueprint)
    {
        return precalculatedSummaries.GetOrInsert(blueprint, blueprint, summarizeBlueprint);
    }

    private PrecalculatedBlueprintSummary summarizeBlueprint(IGameObjectBlueprint blueprint)
    {
        var instantiatedEnemy = EnemyFactory.CreateTemplate(blueprint);

        var sockets = instantiatedEnemy.GetComponents<ISocket>();
        // assumption: if you have multiple sockets of the same shape, they will all receive the same module
        var shapes = sockets.Select(s => s.Shape).Distinct().ToImmutableArray();

        var resistanceContributions = instantiatedEnemy.GetComponents<IResistanceContributions>().SingleOrDefault();

        // assumption: all modules in all sockets must match the affinity element
        var supportedElements = shapes
            .Select(s => modulesBySocket[s].Select(m => m.AffinityElement))
            .Aggregate(allElements, (left, right) => left.Intersect(right));

        return new PrecalculatedBlueprintSummary(shapes, resistanceContributions, supportedElements);
    }

    private record PrecalculatedBlueprintSummary(
        ImmutableArray<SocketShape> SocketShapes,
        IResistanceContributions? ResistanceContributions,
        ImmutableHashSet<Element> SupportedElements);
}
