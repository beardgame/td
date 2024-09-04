using System;
using System.Reactive.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.UI;
using Bearded.TD.UI.Controls;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Rules.Resources;

[GameRule("exchangeCoreEnergyToScrap")]
sealed class ExchangeCoreEnergyToScrap : GameRule<ExchangeCoreEnergyToScrap.RuleParameters>
{
    private FactionResources? resources;
    private FactionCoreEnergyExchange? exchange;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ExchangeCoreEnergyToScrap(RuleParameters parameters) : base(parameters)
    {
    }

    [UsedImplicitly]
    public sealed record RuleParameters(
        ExternalId<Faction> Faction,
        double ExchangeRate,
        bool ExcludeOnGameStart
    );

    public override void Execute(GameRuleContext context)
    {
        if (Parameters.ExcludeOnGameStart)
        {
            context.Events.Subscribe<GameStarted>(_ => setup(context));
            return;
        }

        setup(context);
    }

    private void setup(GameRuleContext context)
    {
        var faction = context.Factions.Find(Parameters.Faction);
        faction.TryGetBehaviorIncludingAncestors(out resources);
        faction.TryGetBehaviorIncludingAncestors(out exchange);

        _ = context.Events
            .Observe<ResourcesProvided<CoreEnergy>>()
            .Where(isCorrectFaction)
            .Subscribe(tryExchange);

        bool isCorrectFaction(ResourcesProvided<CoreEnergy> e)
        {
            return resources == e.Resources;
        }

        void tryExchange(ResourcesProvided<CoreEnergy> e)
        {
            var exchangeRate = getExchangeRate();
            var percentageToExchange = getPercentage();

            var coreEnergyGained = e.AmountProvided;
            var coreEnergyToExchange = coreEnergyGained * percentageToExchange;

            var currentCoreEnergy = e.Resources.GetCurrent<CoreEnergy>();
            if (coreEnergyToExchange > currentCoreEnergy)
            {
                coreEnergyToExchange = currentCoreEnergy;
            }

            var scrapToGain = coreEnergyToExchange * exchangeRate;

            e.Resources.Exchange(coreEnergyToExchange, scrapToGain);
        }
    }

    private double getPercentage()
    {
        return exchange?.ExchangePercentage ?? 1;
    }

    private ExchangeRate<CoreEnergy, Scrap> getExchangeRate()
    {
        return ExchangeRate.FromTo(1.CoreEnergy(), Parameters.ExchangeRate.Scrap());
    }
}
