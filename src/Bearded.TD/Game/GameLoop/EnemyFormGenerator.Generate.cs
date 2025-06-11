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

        var resistances = summary.ResistanceContributions is null
            ? ImmutableDictionary<DamageType, Resistance>.Empty
            : deriveResistances(maybeAssignedModules, summary.ResistanceContributions);
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

    private ImmutableDictionary<DamageType, Resistance> deriveResistances(
        ImmutableDictionary<SocketShape, IModule> modules, IResistanceContributions resistanceContributions)
    {
        var builder = ImmutableDictionary.CreateBuilder<DamageType, Resistance>();

        foreach (var (socketShape, resistance) in resistanceContributions.Factors)
        {
            if (!modules.TryGetValue(socketShape, out var module))
            {
                logger.Warning?.Log(
                    $"Attempted to calculate damage resistance derived from socket {socketShape} but no assigned " +
                    $"module was found.");
                continue;
            }

            var damageType = module.AffinityElement.ToDamageType();
            var existingResistance = builder.GetValueOrDefault(damageType);
            builder[damageType] = existingResistance + resistance;
        }

        return builder.ToImmutable();
    }
}
