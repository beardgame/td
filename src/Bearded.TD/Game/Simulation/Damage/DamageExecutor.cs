using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Damage;

readonly struct DamageExecutor
{
    private readonly IDamageSource? damageSource;

    private DamageExecutor(IDamageSource? damageSource)
    {
        this.damageSource = damageSource;
    }

    public bool TryDoDamage(IComponentOwner target, DamageInfo damage)
    {
        if (!target.TryGetSingleComponent<IHealthEventReceiver>(out var receiver))
        {
            return false;
        }

        receiver.Damage(damage, damageSource);
        return true;
    }

    public static DamageExecutor FromObject(IComponentOwner source)
    {
        source.TryGetSingleComponentInOwnerTree<IDamageSource>(out var damageSource);
        return FromDamageSource(damageSource);
    }

    public static DamageExecutor FromDamageSource(IDamageSource? source)
    {
        return new DamageExecutor(source);
    }
}