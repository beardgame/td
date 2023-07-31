using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Generation;

readonly struct AccumulateNodeGroups : IGlobalEvent
{
    private readonly Accumulator<NodeGroup> accumulator;

    public AccumulateNodeGroups(Accumulator<NodeGroup> accumulator)
    {
        this.accumulator = accumulator;
    }

    public void AddNodeGroup(NodeGroup nodeGroup)
    {
        accumulator.Contribute(nodeGroup);
    }
}
