using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Drawing.ShakeScreenOnHit;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing;

[Component("shakeScreenOnHit")]
sealed class ShakeScreenOnHit(IParameters parameters)
    : Component<IParameters>(parameters), IListener<ObjectHit>, IListener<CollidedWithLevel>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1, Type = AttributeType.ScreenShakeStrength)]
        float Strength { get; }
        [Modifiable(1)]
        TimeSpan Duration { get; }
        [Modifiable(10)]
        Frequency Frequency { get; }
        bool RandomDirection { get; }
    }

    protected override void OnAdded()
    {
        Events.Subscribe<CollidedWithLevel>(this);
        Events.Subscribe<ObjectHit>(this);
    }

    protected override void OnRemovedInternal()
    {
        Events.Unsubscribe<CollidedWithLevel>(this);
        Events.Unsubscribe<ObjectHit>(this);
    }

    public void HandleEvent(ObjectHit _)
    {
        onHit();
    }

    public void HandleEvent(CollidedWithLevel _)
    {
        onHit();
    }

    private void onHit()
    {
        var shake = new ScreenShakeParameters(
            Owner.Position,
            Parameters.RandomDirection ? Direction2.FromDegrees(Random.Shared.NextFloat(360)) : Owner.Direction,
            Parameters.Strength,
            Parameters.Duration,
            Parameters.Frequency);

        Owner.Game.Meta.ScreenShaker.Shake(shake);
    }
}
