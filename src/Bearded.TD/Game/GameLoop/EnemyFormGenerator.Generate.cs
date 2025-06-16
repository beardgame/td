using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Game.GameLoop;

sealed partial class EnemyFormGenerator
{
    public EnemyForm Generate(IGameObjectBlueprint blueprint, Requirements requirements, Random random)
    {
        var summary = findSummary(blueprint);

        var maybeAssignedModules = assignModulesToShapes(summary.SocketShapes, requirements, random);
        if (maybeAssignedModules is null)
        {
            throw new InvalidOperationException(
                "Could not assign modules. Ensure you check CanGenerate before generating an enemy form");
        }

        var resistances = DamageResistancesCalculator.DeriveResistancesFromModules(
            maybeAssignedModules, summary.ResistanceContributions);
        return new EnemyForm(blueprint, maybeAssignedModules, resistances);
    }

    private ImmutableDictionary<SocketShape, IModule>? assignModulesToShapes(
        IEnumerable<SocketShape> shapes, Requirements requirements, Random random)
    {
        var builder = ImmutableDictionary.CreateBuilder<SocketShape, IModule>();
        foreach (var s in shapes)
        {
            var moduleMaybe = chooseAppropriateModule(modulesBySocket[s], requirements, random);
            if (moduleMaybe is null)
            {
                return null;
            }

            builder[s] = moduleMaybe;
        }

        return builder.ToImmutable();
    }

    private static IModule? chooseAppropriateModule(
        IEnumerable<IModule> modules, Requirements requirements, Random random)
    {
        var appropriateModules =
            modules.Where(m => m.AffinityElement == requirements.AffinityElement).ToImmutableArray();
        return appropriateModules.IsEmpty ? null : appropriateModules.RandomElement(random);
    }
}
