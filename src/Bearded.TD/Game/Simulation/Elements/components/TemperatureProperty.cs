using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.StatusDisplays;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.Elements;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Elements;

[Trigger("overheated")]
record struct Overheated : IComponentEvent;
[Trigger("stopOverheated")]
record struct StopOverheated : IComponentEvent;
[Trigger("frozen")]
record struct Frozen : IComponentEvent;
[Trigger("stopFrozen")]
record struct StopFrozen : IComponentEvent;

[Component("temperature")]
sealed partial class TemperatureProperty : Component, IProperty<Temperature>, ITemperatureEventReceiver
{
    public Temperature Value { get; private set; }
    private TemperatureState currentState = TemperatureState.Normal;

    private TickCycle? tickCycle;
    private IStatusTracker? statusDisplay;
    private ITilePresenceListener? tilePresenceListener;
    private TemperatureDifference queuedTemperatureChange;

    protected override void OnAdded() { }

    public override void Activate()
    {
        base.Activate();
        tickCycle = new TickCycle(Owner.Game, applyTick);
        Owner.TryGetSingleComponent(out statusDisplay);
        tilePresenceListener = Owner.TrackTilePresenceInLayer(Owner.Game.TemperatureLayer);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        tickCycle?.Update();
    }

    protected override void OnRemovedInternal()
    {
        tilePresenceListener?.Detach();
        base.OnRemovedInternal();
    }

    public void ApplyImmediateTemperatureChange(TemperatureDifference difference)
    {
        queuedTemperatureChange += difference;
    }

    private void applyTick(Instant now)
    {
        applyChanges(now);
        updateEffects();
        applyDecay();
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
        Value = SpaceTime1MathF.Clamp(Value, MinTemperature, MaxTemperature);
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

    private void updateEffects()
    {
        var newState = 0 switch
        {
            _ when Value < MinNormalTemperature => TemperatureState.Cold,
            _ when Value > MaxNormalTemperature => TemperatureState.Hot,
            _ => TemperatureState.Normal,
        };

        if (currentState != newState)
        {
            currentState.Stop(this);
            currentState = newState;
            currentState.Start(this);
        }

        currentState.Update(this);
    }
}
