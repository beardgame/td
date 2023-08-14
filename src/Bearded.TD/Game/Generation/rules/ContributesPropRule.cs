using System.Collections.Immutable;
using Bearded.TD.Game.Generation.Semantic.Props;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;
using static Bearded.TD.Game.Generation.ObjectPositioning;

namespace Bearded.TD.Game.Generation;

[GameRule("contributePropRule")]
sealed class ContributesPropRule : GameRule<ContributesPropRule.RuleParameters>, IListener<AccumulatePropRules>
{
    public ContributesPropRule(RuleParameters parameters) : base(parameters) { }

    public override void Execute(GameRuleContext context)
    {
        if (Parameters.Solution.Blueprints.IsDefaultOrEmpty)
        {
            context.Logger.Warning?.Log("Cannot contribute a prop rule without blueprints.");
            return;
        }
        context.Events.Subscribe(this);
    }

    public void HandleEvent(AccumulatePropRules @event)
    {
        @event.AddPropRule(new PropRule(Parameters.Selector.ToSelector(), Parameters.Solution.ToFactory()));
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
    public record RuleParameters(Selector Selector, Solution Solution);

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
    public record Selector(PropPurpose Purpose)
    {
        public PropRuleSelector ToSelector() => new(Purpose);
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
    public record Solution(
        ImmutableArray<IGameObjectBlueprint> Blueprints, AlignmentMode Alignment, RotationMode Rotation, Unit Z)
    {
        public IPropSolutionFactory ToFactory() =>
            new GameObjectPropSolutionFactory(Blueprints, Alignment, Rotation, Z);
    }
}
