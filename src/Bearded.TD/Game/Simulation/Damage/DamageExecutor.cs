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

        // TODO: this result needs to include how much damage was CONSUMED
        var damageResult =
            damageReceiver?.Damage(typedDamage, damageSource) ?? FinalDamageResult.None(typedDamage.Type);

        // TODO: attribute damage against consumed (not potential)
        // anything else later should be tracked as 'misses'/accuracy
        damageSource?.AttributeDamage(damageResult, target);

        // TODO: return damage result to reduce damage potential in the caller of this method, e.g. DamageOnObjectHit
        // those callers should know how to do that correctly, and support skipping the consumption if they are 'additional damage thingies'
        // (may need another preview event to allow source to modify damage potential lost)

        // TODO: check if this can be taken outside this method so the hit parameter can be removed
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
