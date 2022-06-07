using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("hitLevelAtTower")]
class HitLevelAtTower : WeaponCycleHandler<HitLevelAtTower.IParameters>
{
    private Instant nextPossibleShootTime;
    private bool firstShotInBurst = true;

    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1, Type = AttributeType.FireRate)]
        Frequency FireRate { get; }
    }

    public HitLevelAtTower(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
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
        var point = Owner.Parent is GameObject { Position: var p }
            ? p
            : Owner.Position;

        Events.Send(new HitLevel(new HitInfo(point, new Difference3(0, 0, 1), new Difference3(0, 0, -1))));
    }
}
