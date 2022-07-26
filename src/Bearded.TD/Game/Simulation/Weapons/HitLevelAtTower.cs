using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("hitLevelAtTower")]
sealed class HitLevelAtTower : Component, IListener<ShootProjectile>
{
    private DynamicDamage? damageProvider;

    protected override void OnAdded()
    {
        damageProvider = new DynamicDamage();
        Owner.AddComponent(damageProvider);
        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        if (damageProvider != null)
        {
            Owner.RemoveComponent(damageProvider);
        }

        base.OnRemoved();
    }

    public override void Update(TimeSpan elapsedTime)
    {
        
    }

    public void HandleEvent(ShootProjectile @event)
    {
        hitLevel(@event.Damage);
    }


    private void hitLevel(UntypedDamage damage)
    {
        DebugAssert.State.Satisfies(damageProvider != null);
        damageProvider?.Inject(damage);
        var point = Owner.Parent is GameObject { Position: var p }
            ? p
            : Owner.Position;

        Events.Send(new HitLevel(new HitInfo(point, new Difference3(0, 0, 1), new Difference3(0, 0, -1))));
    }
}
