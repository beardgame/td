using System;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements;

sealed class HitTargetOnActivate : Component
{
    protected override void OnAdded() { }

    public override void Activate()
    {
        base.Activate();

        if (!Owner.TryGetSingleComponent<IProperty<Target>>(out var targetProperty))
        {
            throw new InvalidOperationException("Cannot hit target if no target is set.");
        }

        var target = targetProperty.Value.Object;

        var ray = new Ray3(Owner.Position, target.Position);
        var rayCastResults = Owner.Game.Level.CastPiercingRayAgainstObjects(ray, Owner.Game.PhysicsLayer, _ => true);
        var resultForTarget = rayCastResults.FirstOrDefault(r => r.Object == target);
        if (resultForTarget.Object != target)
        {
            Owner.Game.Meta.Logger.Warning?.Log("Enemy not hit by ray pointing to enemy.");
        }

        var hitInfo = resultForTarget.Object == target
            ? new Impact(resultForTarget.Point, resultForTarget.Normal!.Value, ray.Direction)
            : new Impact(target.Position, new Difference3(Vector3.UnitZ), ray.Direction);

        Events.Send(new TouchObject(target, hitInfo));
    }

    public override void Update(TimeSpan elapsedTime) { }
}
