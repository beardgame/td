using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Damage;

readonly struct DamageExecutor
{
    private readonly IDamageSource? damageSource;

    private DamageExecutor(IDamageSource? damageSource)
    {
        this.damageSource = damageSource;
    }

    public bool TryDoDamage(GameObject target, TypedDamage typedDamage, Hit hit)
    {
        // Affect actual health
        target.TryGetSingleComponent<IHealthEventReceiver>(out var damageReceiver);
        var damageResult =
            damageReceiver?.Damage(typedDamage, damageSource) ?? FinalDamageResult.None(typedDamage.Type);
        damageSource?.AttributeDamage(damageResult, target);

        // Inject information about having been hit by something, whether damage was done or not
        // Typically used for visual effects.
        target.TryGetSingleComponent<IEventReceiver<TakeHit>>(out var hitReceiver);
        hitReceiver?.InjectEvent(new TakeHit(hit, damageResult.TotalExactDamage));

        return damageReceiver != null;
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

    public static DamageExecutor WithoutDamageSource()
    {
        return new DamageExecutor(null);
    }
}
