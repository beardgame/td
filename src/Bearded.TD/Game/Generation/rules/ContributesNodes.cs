using System.Collections.Immutable;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Shared.Events;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Generation;

[GameRule("contributeNodes")]
sealed class ContributesNodes : GameRule<ContributesNodes.RuleParameters>, IListener<AccumulateNodeGroups>
{
    public ContributesNodes(RuleParameters parameters) : base(parameters) { }

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

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
    public sealed record RuleParameters(ImmutableArray<NodeGroup> Nodes);
}
