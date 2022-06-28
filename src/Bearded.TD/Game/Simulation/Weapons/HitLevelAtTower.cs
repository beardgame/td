using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("hitLevelAtTower")]
sealed class HitLevelAtTower : WeaponCycleHandler<HitLevelAtTower.IParameters>
{
    private Instant nextPossibleShootTime;
    private bool firstShotInBurst = true;
    private DynamicDamage? damageProvider;

    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.Damage)]
        UntypedDamagePerSecond DamagePerSecond { get; }

        [Modifiable(1, Type = AttributeType.FireRate)]
        Frequency FireRate { get; }
    }

    public HitLevelAtTower(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        base.OnAdded();
        damageProvider = new DynamicDamage();
        Owner.AddComponent(damageProvider);
    }

    public override void OnRemoved()
    {
        if (damageProvider != null)
        {
            Owner.RemoveComponent(damageProvider);
        }

        base.OnRemoved();
    }

    protected override void UpdateIdle(TimeSpan elapsedTime)
    {
        firstShotInBurst = true;
    }

    protected override void UpdateShooting(TimeSpan elapsedTime)
    {
        var currentTime = Game.Time;
        while (nextPossibleShootTime < currentTime)
        {
            hitLevel();

            if (firstShotInBurst)
            {
                nextPossibleShootTime = currentTime + 1 / Parameters.FireRate;
                firstShotInBurst = false;
            }
            else
            {
                nextPossibleShootTime += 1 / Parameters.FireRate;
            }
        }
    }

    private void hitLevel()
    {
        DebugAssert.State.Satisfies(damageProvider != null);
        damageProvider?.Inject(Parameters.DamagePerSecond / Parameters.FireRate);
        var point = Owner.Parent is GameObject { Position: var p }
            ? p
            : Owner.Position;

        Events.Send(new HitLevel(new HitInfo(point, new Difference3(0, 0, 1), new Difference3(0, 0, -1))));
    }
}
