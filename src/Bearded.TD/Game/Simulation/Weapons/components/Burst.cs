using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("burst")]
sealed class Burst : Component<Burst.IParameters>, IListener<ShotProjectile>, IPreviewListener<PreviewDelayNextShot>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        public int BurstSize { get; }

        public double FireRateFactor { get; }

        public double OverallDamageFactor { get; }
    }

    private int shotsFiredInBurst;
    private Instant lastShotFired = Instant.Zero;
    private TimeSpan lastRecordedDelay = TimeSpan.Zero;

    private double correctionFactor => 1 / Parameters.OverallDamageFactor - 1 / Parameters.FireRateFactor;
    private TimeSpan delayBetweenBursts => lastRecordedDelay * Parameters.BurstSize * correctionFactor;

    public Burst(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        Events.Subscribe<ShotProjectile>(this);
        Events.Subscribe<PreviewDelayNextShot>(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        // Reset burst if longer than the expected burst delay has passed.
        if (shotsFiredInBurst > 0 && Owner.Game.Time - lastShotFired >= delayBetweenBursts)
        {
            shotsFiredInBurst = 0;
        }
    }

    public override void OnRemoved()
    {
        base.OnRemoved();
        Events.Unsubscribe<ShotProjectile>(this);
        Events.Unsubscribe<PreviewDelayNextShot>(this);
    }

    public void HandleEvent(ShotProjectile @event)
    {
        shotsFiredInBurst++;
        lastShotFired = Owner.Game.Time;
    }

    public void PreviewEvent(ref PreviewDelayNextShot @event)
    {
        lastRecordedDelay = @event.Delay;
        if (shotsFiredInBurst < Parameters.BurstSize)
        {
            @event = new PreviewDelayNextShot(@event.Delay / Parameters.FireRateFactor);
            return;
        }

        @event = new PreviewDelayNextShot(delayBetweenBursts);
        shotsFiredInBurst = 0;
    }
}
