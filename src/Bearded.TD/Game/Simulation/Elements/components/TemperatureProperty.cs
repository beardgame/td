using Bearded.TD.Game.Simulation.Elements.events;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.Elements;

namespace Bearded.TD.Game.Simulation.Elements;

[Component("temperature")]
sealed class TemperatureProperty : Component, IProperty<Temperature>
{
    public Temperature Value { get; private set; }
    private TickCycle? tickCycle;

    protected override void OnAdded() { }

    public override void Activate()
    {
        base.Activate();
        tickCycle = new TickCycle(Owner.Game, applyTick);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        tickCycle?.Update();
    }

    private void applyTick(Instant now)
    {
        applyChanges(now);
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
}
