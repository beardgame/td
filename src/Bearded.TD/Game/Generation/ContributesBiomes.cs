using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.Events;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Generation;

[GameRule("contributeBiomes")]
sealed class ContributesBiomes : GameRule<ContributesBiomes.RuleParameters>, IListener<AccumulateBiomes>
{
    public ContributesBiomes(RuleParameters parameters) : base(parameters) { }

    public override void Execute(GameRuleContext context)
    {
        context.Events.Subscribe(this);
    }

    public void HandleEvent(AccumulateBiomes @event)
    {
        foreach (var biome in Parameters.Biomes)
        {
            @event.ContributeBiome(biome);
        }
    }


    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
    public sealed record RuleParameters(ImmutableArray<IBiome> Biomes);
}
