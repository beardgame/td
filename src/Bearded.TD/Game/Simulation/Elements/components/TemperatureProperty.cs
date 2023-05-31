using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.Elements.events;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.Elements;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("temperature")]
sealed class TemperatureProperty : Component, IProperty<Temperature>, ITemperatureEventReceiver
{
    public Temperature Value { get; private set; }
    private TickCycle? tickCycle;
    private IBreakageReceipt? breakage;
    private ITilePresenceListener? tilePresenceListener;
    private TemperatureDifference queuedTemperatureChange;

    protected override void OnAdded() { }

    public override void Activate()
    {
        base.Activate();
        tickCycle = new TickCycle(Owner.Game, applyTick);
        tilePresenceListener = Owner.TrackTilePresenceInLayer(Owner.Game.TemperatureLayer);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        tickCycle?.Update();
    }

    public override void OnRemoved()
    {
        tilePresenceListener?.Detach();
        base.OnRemoved();
    }

    private void applyTick(Instant now)
    {
        applyChanges(now);
        applyDecay();
        updateBreakage();
    }

    private void applyChanges(Instant now)
    {
        var @event = new PreviewTemperatureTick(now, TemperatureRate.Zero);
        Events.Preview(ref @event);
        if (@event.Rate != TemperatureRate.Zero)
        {
            Value += @event.Rate * TickDuration;
        }

        Value += queuedTemperatureChange;
        queuedTemperatureChange = TemperatureDifference.Zero;
    }

    private void applyDecay()
    {
        if (Value > AmbientTemperature)
        {
            Value = SpaceTime1MathF.Max(AmbientTemperature, Value - TickDuration * TemperatureDecayRate);
        }
        if (Value < AmbientTemperature)
        {
            Value = SpaceTime1MathF.Min(AmbientTemperature, Value + TickDuration * TemperatureDecayRate);
        }
    }

    private void updateBreakage()
    {
        if (breakage is { } receipt && Value <= MaxNormalTemperature)
        {
            receipt.Repair();
            breakage = null;
        }

        if (breakage is null &&
            Value > MaxShownTemperature &&
            Owner.TryGetSingleComponent<IBreakageHandler>(out var breakageHandler))
        {
            breakage = breakageHandler.BreakObject();
        }
    }

    public void ApplyImmediateTemperatureChange(TemperatureDifference difference)
    {
        queuedTemperatureChange += difference;
    }
}
