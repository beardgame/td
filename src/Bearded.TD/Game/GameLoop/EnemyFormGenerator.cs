using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Game.GameLoop;

sealed class EnemyFormGenerator
{
    private readonly ILookup<SocketShape, IModule> modulesBySocket;

    private readonly Random random;
    private readonly Logger logger;

    public EnemyFormGenerator(IEnumerable<IModule> modules, Random random, Logger logger)
    {
        modulesBySocket = modules.ToLookup(m => m.SocketShape);
        this.random = random;
        this.logger = logger;
    }

    public bool TryGenerateEnemyForm(
        IGameObjectBlueprint blueprint, Requirements requirements, [NotNullWhen(true)] out EnemyForm? form)
    {
        var instantiatedEnemy = EnemyUnitFactory.Create(Id<GameObject>.Invalid, blueprint, Tile.Origin);
        var sockets = instantiatedEnemy.GetComponents<ISocket>();
        // assumption: if you have multiple sockets of the same shape, they will all receive the same module
        var shapes = sockets.Select(s => s.Shape).Distinct();

        var maybeAssignedModules = assignModulesToShapes(shapes, requirements);
        if (maybeAssignedModules is null)
        {
            form = default;
            return false;
        }

        var resistanceContributions = instantiatedEnemy.GetComponents<IResistanceContributions>().SingleOrDefault();
        var resistances = resistanceContributions is null
            ? ImmutableDictionary<DamageType, Resistance>.Empty
            : deriveResistances(maybeAssignedModules, resistanceContributions);
        form = new EnemyForm(blueprint, maybeAssignedModules, resistances);
        return true;
    }

    private ImmutableDictionary<SocketShape, IModule>? assignModulesToShapes(
        IEnumerable<SocketShape> shapes, Requirements requirements)
    {
        var builder = ImmutableDictionary.CreateBuilder<SocketShape, IModule>();
        foreach (var s in shapes)
        {
            var moduleMaybe = chooseAppropriateModule(modulesBySocket[s], requirements);
            if (moduleMaybe is null)
            {
                return null;
            }
            builder[s] = moduleMaybe;
        }

        return builder.ToImmutable();
    }

    private IModule? chooseAppropriateModule(IEnumerable<IModule> modules, Requirements requirements)
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

    public readonly record struct Requirements(Element AffinityElement);
}
