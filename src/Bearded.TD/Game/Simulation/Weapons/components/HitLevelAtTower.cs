using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("hitLevelAtTower")]
sealed class HitLevelAtTower(HitLevelAtTower.IParameters parameters)
    : Component<HitLevelAtTower.IParameters>(parameters), IListener<FireWeapon>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        Difference3 Offset { get; }
    }

    private DynamicDamage? damageProvider;

    protected override void OnAdded()
    {
        damageProvider = new DynamicDamage();
        Owner.AddComponent(damageProvider);
        Events.Subscribe(this);
    }

    protected override void OnRemoved()
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

    public void HandleEvent(FireWeapon @event)
    {
        hitLevel(@event.Damage);
    }

    private void hitLevel(UntypedDamage damage)
    {
        DebugAssert.State.Satisfies(damageProvider != null);
        damageProvider?.Inject(damage);
        var point = Owner.Parent is { Position: var p }
            ? p
            : Owner.Position;

        var direction = Owner.Direction.Vector;
        var offset = Parameters.Offset;
        point += (offset.X * direction + offset.Y * direction.PerpendicularRight).WithZ(offset.Z);

        Events.Send(new CollidedWithLevel(new Impact(point, new Difference3(0, 0, 1), new Difference3(0, 0, -1)), Level.GetTile(point)));
    }
}
