using System.Collections.Generic;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Rules;

namespace Bearded.TD.Game.Generation
{
    [GameRule("contributeNodes")]
    sealed class ContributesNodesGroup : GameRule<ContributesNodesGroup.RuleParameters>, IListener<AccumulateNodeGroups>
    {
        public ContributesNodesGroup(RuleParameters parameters) : base(parameters) { }

        public override void Execute(GameRuleContext context)
        {
            context.Events.Subscribe(this);
        }

        public void HandleEvent(AccumulateNodeGroups @event)
        {
            foreach (var n in Parameters.Nodes)
            {
                @event.AddNodeGroup(n);
            }
        }

        public sealed record RuleParameters(List<NodeGroup> Nodes);
    }
}
