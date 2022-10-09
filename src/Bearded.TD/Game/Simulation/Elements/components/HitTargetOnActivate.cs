using System;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
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
        var rayCastResults = Owner.Game.Level.CastPiercingRayAgainstEnemies(ray, Owner.Game.UnitLayer, _ => true);
        var resultForTarget = rayCastResults.FirstOrDefault(r => r.Enemy == target);
        if (resultForTarget.Enemy != target)
        {
            Owner.Game.Meta.Logger.Warning?.Log("Enemy not hit by ray pointing to enemy.");
        }

        var hitInfo = resultForTarget.Enemy == target
            ? new HitInfo(resultForTarget.Point, resultForTarget.Normal!.Value, ray.Direction)
            : new HitInfo(target.Position, new Difference3(Vector3.UnitZ), ray.Direction);

        Events.Send(new HitEnemy(target, hitInfo));
    }

    public override void Update(TimeSpan elapsedTime) { }
}
