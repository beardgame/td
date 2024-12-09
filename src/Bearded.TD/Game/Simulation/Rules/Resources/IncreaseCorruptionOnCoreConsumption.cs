using System;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Core;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.Events;
using JetBrains.Annotations;
using static Bearded.TD.Game.Simulation.Rules.Resources.IncreaseCorruptionOnCoreConsumption;

namespace Bearded.TD.Game.Simulation.Rules.Resources;

[GameRule("increaseCorruptionOnCoreConsumption")]
sealed class IncreaseCorruptionOnCoreConsumption(RuleParameters parameters)
    : GameRule<RuleParameters>(parameters), IListener<ResourcesConsumed<CoreEnergy>>
{
    private FactionResources resources = null!;
    private GameState game = null!;

    [UsedImplicitly]
    public sealed record RuleParameters(
        ExternalId<Faction> Faction,
        double BaseRate = 1,
        double PerDebtRate = 0.01
    );

    public override void Execute(GameRuleContext context)
    {
        var faction = context.Factions.Find(Parameters.Faction);
        if (!faction.TryGetBehaviorIncludingAncestors(out resources!))
        {
            context.Logger.Warning?.Log($"Expected faction {Parameters.Faction} to have resources, but it does not.");
            return;
        }

        context.Events.Subscribe(this);
        game = context.GameState;
    }

    public void HandleEvent(ResourcesConsumed<CoreEnergy> e)
    {
        if (e.Resources != resources)
            return;

        var consumed = e.AmountConsumed;
        var current = resources.GetCurrent<CoreEnergy>();
        var debt = Math.Max(0, -current.Value);
        var rate = Parameters.BaseRate + Parameters.PerDebtRate * debt;

        var corruption = new Corruption(consumed.Value * rate);

        game.Corruption.AddCorruption(corruption);
    }
}
