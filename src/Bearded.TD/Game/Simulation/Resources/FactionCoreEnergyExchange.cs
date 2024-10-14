using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Resources;

[FactionBehavior("coreEnergyExchange")]
sealed class FactionCoreEnergyExchange : FactionBehavior
{
    public double Percentage { get; private set; } = 0.75;
    public ExchangeRate<CoreEnergy, Scrap> Rate { get; private set; } = ExchangeRate.FromTo(2.CoreEnergy(), 1.Scrap());

    protected override void Execute()
    {
    }

    public void SetExchangePercentage(double percentage)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (Percentage == percentage)
            return;

        Percentage = percentage;
        Events.Send(new ExchangePercentageChanged(this, percentage));
    }

    public void SetExchangeRate(ExchangeRate<CoreEnergy, Scrap> rate)
    {
        if (Rate == rate)
            return;

        Rate = rate;
        Events.Send(new ExchangeRateChanged(this, rate));
    }
}

internal record struct ExchangePercentageChanged(FactionCoreEnergyExchange Exchange, double Percentage) : IGlobalEvent;
internal record struct ExchangeRateChanged(FactionCoreEnergyExchange Exchange, ExchangeRate<CoreEnergy, Scrap> Rate) : IGlobalEvent;
