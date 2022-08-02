using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("spinUp")]
sealed class SpinUp : Component<SpinUp.IParameters>, IPreviewListener<PreviewDelayNextShot>
{
    private IWeaponTrigger? trigger;
    private float speedFactor;

    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(0.1)]
        double MinimumSpeedFactor { get; }
        [Modifiable(1)]
        double MaximumSpeedFactor { get; }
        [Modifiable(1)]
        TimeSpan SpeedUpTime { get; }
        [Modifiable(1)]
        TimeSpan SlowDownTime { get; }
    }

    public SpinUp(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        ComponentDependencies.DependDynamic<IWeaponTrigger>(Owner, Events, c => trigger = c);
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (trigger?.TriggerPulled ?? false)
        {
            modifySpeed(Parameters.SpeedUpTime, elapsedTime);
        }
        else
        {
            modifySpeed(-Parameters.SlowDownTime, elapsedTime);
        }
    }

    private void modifySpeed(TimeSpan zeroToOneTime, TimeSpan elapsedTime)
    {
        var speedStep = 1 / zeroToOneTime * elapsedTime;

        speedFactor = (speedFactor + (float)speedStep).Clamped(0, 1);
    }

    public void PreviewEvent(ref PreviewDelayNextShot e)
    {
        // TODO: doesn't currently anticipate additional speeding up in the meantime
        var delayFactor = 1 / Interpolate.Lerp(
            (float)Parameters.MinimumSpeedFactor,
            (float)Parameters.MaximumSpeedFactor,
            speedFactor);

        DebugAssert.State.Satisfies(float.IsFinite(delayFactor));
        DebugAssert.State.Satisfies(delayFactor > 0);

        e = new PreviewDelayNextShot(e.Delay * delayFactor);
    }
}
