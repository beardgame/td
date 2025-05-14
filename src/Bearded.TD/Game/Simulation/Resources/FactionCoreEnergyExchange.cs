using System;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Resources;

readonly record struct ExchangePreview(
    Resource<CoreEnergy> CoreEnergyIn,
    Resource<CoreEnergy> CoreEnergyExchanged,
    Resource<Scrap> ScrapChange,
    double MaximumExchangePercentage)
{
    public Resource<CoreEnergy> CoreEnergyChanged => CoreEnergyIn - CoreEnergyExchanged;
}

[FactionBehavior("coreEnergyExchange")]
sealed class FactionCoreEnergyExchange : FactionBehavior
{
    private FactionResources? resources;

    public double Percentage { get; private set; } = 0.75;
    public ExchangeRate<CoreEnergy, Scrap> Rate { get; private set; } = ExchangeRate.FromTo(2.CoreEnergy(), 1.Scrap());

    protected override void Execute()
    {
        Owner.TryGetBehaviorIncludingAncestors(out resources);

        _ = Events.Subscribe<ResourcesProvidedPreview<CoreEnergy>>(tryExchange);

    }

    private void tryExchange(ref ResourcesProvidedPreview<CoreEnergy> e)
    {
        if (e.Resources != resources)
            return;

        var changes = PreviewExchange(e.AmountProvided);

        if (changes.CoreEnergyExchanged >= e.AmountProvided)
        {
            resources!.ConsumeResources(changes.CoreEnergyExchanged - e.AmountProvided);
            e = e with { AmountProvided = 0.CoreEnergy() };
        }
        else
        {
            e = e with { AmountProvided = e.AmountProvided - changes.CoreEnergyExchanged };
        }

        resources!.ProvideResources(changes.ScrapChange);
        Events.Send(new ResourcesExchanged<CoreEnergy, Scrap>(resources, changes.CoreEnergyExchanged, changes.ScrapChange));
    }

    public ExchangePreview PreviewExchange(Resource<CoreEnergy> coreEnergyGained)
    {
        var currentCoreEnergy = resources?.GetCurrent<CoreEnergy>() ?? Resource<CoreEnergy>.Zero;

        var maxExchangePercentage = currentCoreEnergy <= Resource<CoreEnergy>.Zero
            ? 1
            : 1 + currentCoreEnergy / coreEnergyGained;

        var exchangePercentage = Math.Min(Percentage, maxExchangePercentage);

        var coreEnergyExchanged = coreEnergyGained * exchangePercentage;
        var scrapChange = coreEnergyExchanged * Rate;

        return new ExchangePreview(coreEnergyGained, coreEnergyExchanged, scrapChange, maxExchangePercentage);
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

record struct ExchangePercentageChanged(FactionCoreEnergyExchange Exchange, double Percentage) : IGlobalEvent;

record struct ExchangeRateChanged(FactionCoreEnergyExchange Exchange, ExchangeRate<CoreEnergy, Scrap> Rate) : IGlobalEvent;
