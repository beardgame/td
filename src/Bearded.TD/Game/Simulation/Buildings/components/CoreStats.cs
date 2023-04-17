using System;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("coreStats")]
sealed class CoreStats : Component, ICoreStats
{
    private IHealth? health;
    private EmergencyEMP? emp;

    private bool deleted;
    public bool Deleted => deleted || Owner.Deleted;

    public HitPoints CurrentHealth => health?.CurrentHealth ?? HitPoints.Zero;
    public HitPoints MaxHealth => health?.CurrentHealth ?? HitPoints.Zero;

    public EMPStatus EMPStatus =>
        emp == null ? EMPStatus.Absent : (emp.Available ? EMPStatus.Ready : EMPStatus.Recharging);

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IHealth>(Owner, Events, h => health = h);
        ComponentDependencies.Depend<EmergencyEMP>(Owner, Events, e => emp = e);
    }

    public override void OnRemoved()
    {
        deleted = true;
        base.OnRemoved();
    }

    public override void Activate()
    {
        base.Activate();

        Owner.Game.ListAs<ICoreStats>(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void FireEMP()
    {
        if (EMPStatus != EMPStatus.Ready)
        {
            throw new InvalidOperationException("Cannot fire EMP when it is not ready.");
        }
        emp?.Fire();
    }
}
