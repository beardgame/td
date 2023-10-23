using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Utilities;
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

        var sourceToTarget = target.Position - Owner.Position;
        var dir = sourceToTarget.NormalizedSafe();
        var impact = new Impact(target.Position, -dir, dir);

        Events.Send(new TouchObject(target, impact));
    }

    public override void Update(TimeSpan elapsedTime) { }
}
