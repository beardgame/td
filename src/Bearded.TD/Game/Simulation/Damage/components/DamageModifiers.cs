using System;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Modules;

namespace Bearded.TD.Game.Simulation.Damage;

static class DamageModifiers
{
    public delegate void ModifierRemover();

    public static void ModifyShell(DamageShell shell, IDamageModifier modifier, GameObject owner, ComponentEvents events, out ModifierRemover modifierRemover)
    {
        HitPointsPool? foundPool = null;

        var dependencyRef = ComponentDependencies.Depend<HitPointsPool>(owner, events, filter: pool => pool.Shell == shell, consumer:
            pool =>
            {
                pool.AddModifier(modifier);
            });

        modifierRemover = undo;
        return;

        void undo()
        {
            dependencyRef.Dispose();
            foundPool?.RemoveModifier(modifier);
        }
    }

    public static ImmutableArray<Type> Order = [
        typeof(PierceDamageThroughShell),
        typeof(Armor),
        typeof(Shield),
        typeof(DamageResistances),
        typeof(DebugInvulnerable)
    ];
}
