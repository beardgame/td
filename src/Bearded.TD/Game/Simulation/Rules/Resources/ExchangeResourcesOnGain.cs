using System;
using System.Reactive.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.UI;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Rules.Resources;

[GameRule("exchangeCoreEnergyToScrap")]
sealed class ExchangeCoreEnergyToScrap : GameRule<ExchangeCoreEnergyToScrap.RuleParameters>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ExchangeCoreEnergyToScrap(RuleParameters parameters) : base(parameters)
    {
    }

    [UsedImplicitly]
    public sealed record RuleParameters(
        ExternalId<Faction> Faction,
        double Percentage,
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
        _ = context.Events
            .Observe<ResourcesProvided<CoreEnergy>>()
            .Where(isCorrectFaction)
            .Subscribe(tryExchange);

        bool isCorrectFaction(ResourcesProvided<CoreEnergy> e)
        {
            var faction = context.Factions.Find(Parameters.Faction);
            return faction.TryGetBehaviorIncludingAncestors<FactionResources>(out var resources)
                && resources == e.Resources;
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
        return Parameters.Percentage;
    }

    private ExchangeRate<CoreEnergy, Scrap> getExchangeRate()
    {
        return ExchangeRate.FromTo(1.CoreEnergy(), Parameters.ExchangeRate.Scrap());
    }
}
