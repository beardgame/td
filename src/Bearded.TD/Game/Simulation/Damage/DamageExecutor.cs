using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Damage;

readonly struct DamageExecutor
{
    private readonly IDamageSource? damageSource;

    private DamageExecutor(IDamageSource? damageSource)
    {
        this.damageSource = damageSource;
    }

    public bool TryDoDamage(GameObject target, TypedDamage typedDamage, HitContext? context = null)
    {
        var hitReceiver = context != null && target.TryGetSingleComponent<IEventReceiver<TakeHit>>(out var r)
            ? r
            : null;

        if (!target.TryGetSingleComponent<IHealthEventReceiver>(out var receiver))
        {
            hitReceiver?.InjectEvent(
                new TakeHit(context.Value, typedDamage, new TypedDamage(HitPoints.Zero, typedDamage.Type)));
            return false;
        }

        var damageTaken = receiver.Damage(typedDamage, damageSource);
        hitReceiver?.InjectEvent(new TakeHit(context.Value, typedDamage, damageTaken));
        return true;
    }

    public static DamageExecutor FromObject(GameObject source)
    {
        source.TryGetSingleComponentInOwnerTree<IDamageSource>(out var damageSource);
        return FromDamageSource(damageSource);
    }

    public static DamageExecutor FromDamageSource(IDamageSource? source)
    {
        return new DamageExecutor(source);
    }
}
