using System;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Resources;

[FactionBehavior("coreEnergyExchange")]
sealed class FactionCoreEnergyExchange : FactionBehavior
{
    public double ExchangePercentage { get; private set; } = 0.75;

    protected override void Execute()
    {
    }

    public void SetExchangePercentage(double percentage)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (ExchangePercentage == percentage)
            return;

        ExchangePercentage = percentage;
        Events.Send(new ExchangePercentageChanged(this, percentage));
    }
}

internal record struct ExchangePercentageChanged(FactionCoreEnergyExchange Exchange, double Percentage) : IGlobalEvent;
