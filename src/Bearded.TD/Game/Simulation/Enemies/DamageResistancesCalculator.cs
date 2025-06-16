using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Damage;

namespace Bearded.TD.Game.Simulation.Enemies;

static class DamageResistancesCalculator
{
    public static ImmutableDictionary<DamageType, Resistance> DeriveResistancesFromModules(
        ImmutableDictionary<SocketShape, IModule> modules, IResistanceContributions? resistanceContributions)
    {
        if (resistanceContributions is null)
        {
            return ImmutableDictionary<DamageType, Resistance>.Empty;
        }

        var builder = ImmutableDictionary.CreateBuilder<DamageType, Resistance>();

        foreach (var (socketShape, resistance) in resistanceContributions.Factors)
        {
            if (!modules.TryGetValue(socketShape, out var module))
            {
                continue;
            }

            var damageType = module.AffinityElement.ToDamageType();
            var existingResistance = builder.GetValueOrDefault(damageType);
            builder[damageType] = existingResistance + resistance;
        }

        return builder.ToImmutable();
    }
}
