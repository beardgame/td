using System;
using System.Reactive.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Utilities;
using Bearded.UI;

namespace Bearded.TD.UI.Controls;

sealed class CoreEnergyExchange
{
    public (Interval Range, double StepSize) ValidExchangePercentages { get; } = (Interval.FromStartAndEnd(0.25, 1.5), 0.05);
    public Binding<double> ExchangePercentage { get; } = new();

    public IReadonlyBinding<ExchangeRate<CoreEnergy, Scrap>> CoreEnergyToScrapRate { get; private set; } = null!;

    public void Initialize(GameInstance game)
    {
        var faction = game.Me.Faction;

        if (faction.TryGetBehaviorIncludingAncestors<FactionCoreEnergyExchange>(out var exchange))
        {
            game.Meta.Events.Observe<ExchangePercentageChanged>()
                .Where(e => e.Exchange == exchange)
                .Select(e => e.Percentage)
                .StartWith(exchange.ExchangePercentage)
                .Subscribe(ExchangePercentage.SetFromSource);

            ExchangePercentage.ControlUpdated += p =>
            {
                game.Request(ChangeCoreEnergyExchangePercentage.Request(faction, p));
            };
        }

        CoreEnergyToScrapRate = Binding.Create(ExchangeRate.FromTo(2.CoreEnergy(), 1.Scrap()));
    }

}
