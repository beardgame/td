using Bearded.Graphics;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Damage;

[Component("hitPoints")]
sealed partial class HitPointsPool(HitPointsPool.IParameters parameters)
    : Component<HitPointsPool.IParameters>(parameters),
        IDamageReceiver,
        IHitPointsPool,
        ISyncable
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1, Type = AttributeType.Health)]
        HitPoints MaxHitPoints { get; }

        HitPoints? InitialHitPoints { get; }

        DamageShell Shell { get; }

        Color? Color { get; }
    }

    private Color color { get; } = parameters.Color ?? defaultColorForShell(parameters.Shell);

    public HitPoints MaxHitPoints { get; private set; } = parameters.MaxHitPoints;
    public HitPoints CurrentHitPoints { get; private set; } = parameters.InitialHitPoints ?? parameters.MaxHitPoints;
    public DamageShell Shell { get; } = parameters.Shell;

    public override void Activate()
    {
        base.Activate();
        if (!Owner.TryGetSingleComponent<IStatusTracker>(out var statusDisplay))
        {
            return;
        }

        statusDisplay.AddHitPointsBar(new HitPointsBar(this, Shell, color));
    }

    public IntermediateDamageResult ApplyDamage(TypedDamage damage, Hit hit, IDamageSource? source)
    {
        // No hit points remaining, so shell is depleted.
        if (CurrentHitPoints <= HitPoints.Zero)
        {
            return IntermediateDamageResult.PassThrough(damage);
        }

        var modifiedDamage = modifyDamage(damage);
        var result = doDamage(damage, modifiedDamage.DamageToSelf, source);
        foreach (var effect in modifiedDamage.AdditionalEffects)
        {
            effect(result, hit);
        }

        return result with
        {
            DamageOverflow = new TypedDamage(
                result.DamageOverflow.Amount + modifiedDamage.DamageToPassThrough.Amount,
                result.DamageOverflow.Type
            ),
        };
    }

    private IntermediateDamageResult doDamage(
        TypedDamage originalDamage, TypedDamage modifiedDamage, IDamageSource? source)
    {
        // No damage done at all, so the shell is 100% effective at blocking it.
        if (modifiedDamage.Amount <= HitPoints.Zero)
        {
            return IntermediateDamageResult.Blocked(originalDamage);
        }

        var cappedDamage =
            modifiedDamage.WithAdjustedAmount(SpaceTime1MathF.Min(modifiedDamage.Amount, CurrentHitPoints));
        modifyHitPoints(-cappedDamage.Amount, out var damageDoneDiscrete);

        Events.Send(new TookDamage(source));

        return new IntermediateDamageResult(cappedDamage, TypedDamage.Zero(originalDamage.Type), damageDoneDiscrete);
    }

    public void OverrideCurrentHitPoints(HitPoints currentHitPoints)
    {
        CurrentHitPoints = currentHitPoints;
    }

    public void RestoreHitPoints(HitPoints hitPointsChange)
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
        if (Parameters.MaxHitPoints != MaxHitPoints)
        {
            applyNewMaxHealth(Parameters.MaxHitPoints);
        }
    }

    private void applyNewMaxHealth(HitPoints newMax)
    {
        if (newMax > MaxHitPoints)
        {
            CurrentHitPoints += newMax - MaxHitPoints;
            MaxHitPoints = newMax;
        }
        else
        {
            MaxHitPoints = newMax;
            CurrentHitPoints = SpaceTime1MathF.Min(CurrentHitPoints, MaxHitPoints);
        }
    }

    private static Color defaultColorForShell(DamageShell shell)
    {
        return shell switch
        {
            DamageShell.Health => Constants.Game.GameUI.HealthColor,
            DamageShell.Armor => Constants.Game.GameUI.ArmorColor,
            DamageShell.Shield => Constants.Game.GameUI.ShieldColor,
            _ => Color.DeepPink
        };
    }
}

interface IHitPointsPool
{
    HitPoints MaxHitPoints { get; }
    HitPoints CurrentHitPoints { get; }
    DamageShell Shell { get; }
}
