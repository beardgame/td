using Bearded.TD.Game.Generation.Semantic.Props;
using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Generation;

readonly struct AccumulatePropRules : IGlobalEvent
{
    private readonly Accumulator<IPropRule> accumulator;

    public AccumulatePropRules(Accumulator<IPropRule> accumulator)
    {
        this.accumulator = accumulator;
    }

    public void AddPropRule(IPropRule propRule)
    {
        accumulator.Contribute(propRule);
    }
}
