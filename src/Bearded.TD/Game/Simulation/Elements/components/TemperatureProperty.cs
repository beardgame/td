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

    private void applyTick(Instant _)
    {
        applyDecay();
    }

    private void applyDecay()
    {
        if (Value > Temperature.Zero)
        {
            Value = SpaceTime1MathF.Max(Temperature.Zero, Value - TickDuration * TemperatureDecayRate);
        }
        if (Value < Temperature.Zero)
        {
            Value = SpaceTime1MathF.Min(Temperature.Zero, Value + TickDuration * TemperatureDecayRate);
        }
    }
}
