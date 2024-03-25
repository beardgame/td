using Bearded.Graphics;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

abstract partial class HitPointsPool<T> : Component<T>,
    IDamageReceiver,
    IHitPointsPool,
    ISyncable
    where T : IParametersTemplate<T>
{
    public abstract DamageShell Shell { get; }
    protected abstract Color Color { get; }

    protected abstract HitPoints TargetMaxHitPoints { get; }
    public HitPoints MaxHitPoints { get; private set; }
    public HitPoints CurrentHitPoints { get; private set; }

    protected HitPointsPool(T parameters, HitPoints maxHitPoints) : base(parameters)
    {
        MaxHitPoints = maxHitPoints;
        CurrentHitPoints = maxHitPoints;
    }

    public override void Activate()
    {
        base.Activate();
        if (!Owner.TryGetSingleComponent<IStatusTracker>(out var statusDisplay))
        {
            return;
        }

        statusDisplay.AddHitPointsBar(new HitPointsBar(this, Shell, Color));
    }

    public IntermediateDamageResult ApplyDamage(TypedDamage damage, IDamageSource? source)
    {
        // No hit points remaining, so shell is depleted.
        if (CurrentHitPoints <= HitPoints.Zero)
        {
            return IntermediateDamageResult.PassThrough(damage);
        }

        var modifiedDamage = ModifyDamage(damage);

        // No damage done at all, so the shell is 100% effective at blocking it.
        if (modifiedDamage.Amount <= HitPoints.Zero)
        {
            return IntermediateDamageResult.Blocked(damage);
        }

        var cappedDamage =
            modifiedDamage.WithAdjustedAmount(SpaceTime1MathF.Min(modifiedDamage.Amount, CurrentHitPoints));
        modifyHitPoints(-cappedDamage.Amount, out var damageDoneDiscrete);

        var result = new IntermediateDamageResult(cappedDamage, TypedDamage.Zero(damage.Type), damageDoneDiscrete);

        Events.Send(new TookDamage(source));

        return result;
    }

    protected abstract TypedDamage ModifyDamage(TypedDamage damage);

    protected void OverrideCurrentHitPoints(HitPoints currentHitPoints)
    {
        CurrentHitPoints = currentHitPoints;
    }

    protected void RestoreHitPoints(HitPoints hitPointsChange)
    {
        modifyHitPoints(hitPointsChange, out _);
    }

    private void modifyHitPoints(HitPoints hitPointsChange, out HitPoints damageDoneDiscrete)
    {
        var oldHealthDiscrete = CurrentHitPoints.Discrete();
        CurrentHitPoints = SpaceTime1MathF.Clamp(CurrentHitPoints + hitPointsChange, HitPoints.Zero, MaxHitPoints);
        var newHealthDiscrete = CurrentHitPoints.Discrete();
        // This expression may look inverted, but that's because we want the difference as a positive number.
        damageDoneDiscrete = oldHealthDiscrete - newHealthDiscrete;
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (TargetMaxHitPoints != MaxHitPoints)
        {
            applyNewMaxHealth();
        }
    }

    private void applyNewMaxHealth()
    {
        if (TargetMaxHitPoints > MaxHitPoints)
        {
            CurrentHitPoints += TargetMaxHitPoints - MaxHitPoints;
            MaxHitPoints = TargetMaxHitPoints;
        }
        else
        {
            MaxHitPoints = TargetMaxHitPoints;
            CurrentHitPoints = SpaceTime1MathF.Min(CurrentHitPoints, MaxHitPoints);
        }
    }
}

interface IHitPointsPool
{
    HitPoints MaxHitPoints { get; }
    HitPoints CurrentHitPoints { get; }
}
