using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Drawing;

[Component("shakeScreenOnFireWeapon")]
sealed class ShakeScreenOnFireWeapon : Component<ShakeScreenOnFireWeapon.IParameters>, IListener<FireWeapon>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        float Strength { get; }
        [Modifiable(1)]
        TimeSpan Duration { get; }
        [Modifiable(10)]
        Frequency Frequency { get; }
        bool RandomDirection { get; }
    }

    public ShakeScreenOnFireWeapon(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(FireWeapon @event)
    {
        var shake = new ScreenShakeParameters(
            Owner.Position,
            Parameters.RandomDirection ? Direction2.FromDegrees(StaticRandom.Float(360)) : Owner.Direction,
            Parameters.Strength,
            Parameters.Duration,
            Parameters.Frequency);

        Owner.Game.Meta.ScreenShaker.Shake(shake);
    }
}
