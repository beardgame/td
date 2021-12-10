using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.World.Fluids;

struct Flow
{
    public FlowRate FlowRight { get; }
    public FlowRate FlowUpRight { get; }
    public FlowRate FlowUpLeft { get; }

    public Flow(FlowRate flowRight, FlowRate flowUpRight, FlowRate flowUpLeft)
    {
        FlowRight = flowRight;
        FlowUpRight = flowUpRight;
        FlowUpLeft = flowUpLeft;
    }

    public void Deconstruct(out FlowRate flowRight, out FlowRate flowUpRight, out FlowRate flowUpLeft)
    {
        flowRight = FlowRight;
        flowUpRight = FlowUpRight;
        flowUpLeft = FlowUpLeft;
    }
}