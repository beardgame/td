using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.UpdateLoop;

namespace Bearded.TD.Game.Simulation.GameLoop
{
    readonly struct PreviewWaveStart : IGlobalPreviewEvent
    {
        public ImmutableList<IPauseCondition>? BlockingConditions { get; }

        private PreviewWaveStart(ImmutableList<IPauseCondition> blockingConditions)
        {
            BlockingConditions = blockingConditions;
        }

        public PreviewWaveStart BlockedBy(IPauseCondition pauseCondition)
        {
            return new PreviewWaveStart(
                (BlockingConditions ?? ImmutableList<IPauseCondition>.Empty).Add(pauseCondition));
        }
    }
}
