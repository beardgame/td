using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("automaticFireCycle")]
sealed class AutomaticFireCycle : WeaponCycleHandler<AutomaticFireCycle.IParameters>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.Damage)]
        UntypedDamagePerSecond DamagePerSecond { get; }

        [Modifiable(1, Type = AttributeType.FireRate)]
        Frequency FireRate { get; }

    }

    private Instant nextPossibleShootTime;
    private bool firstShotInBurst = true;

    public AutomaticFireCycle(IParameters parameters)
        : base(parameters)
    {
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
            emitProjectile();

            var previewDelayEvent = new PreviewDelayNextShot(1 / Parameters.FireRate);
            Events.Preview(ref previewDelayEvent);

            if (firstShotInBurst)
            {
                nextPossibleShootTime = currentTime + previewDelayEvent.Delay;
                firstShotInBurst = false;
            }
            else
            {
                nextPossibleShootTime += previewDelayEvent.Delay;
            }
        }
    }

    private void emitProjectile()
    {
        Events.Send(new ShootProjectile(Parameters.DamagePerSecond / Parameters.FireRate));
    }
}
