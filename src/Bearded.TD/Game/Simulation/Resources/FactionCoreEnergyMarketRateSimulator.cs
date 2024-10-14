using System;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.UI;

namespace Bearded.TD.Game.Simulation.Resources;

[FactionBehavior("coreEnergyMarketRateSimulator")]
sealed class FactionCoreEnergyMarketRateSimulator
    : FactionBehavior<FactionCoreEnergyMarketRateSimulator.FactionParameters>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public FactionCoreEnergyMarketRateSimulator(FactionParameters parameters) : base(parameters)
    {
    }

    public readonly record struct FactionParameters(
        ExchangeRate<CoreEnergy, Scrap> BaseRate,
        ExchangeRate<CoreEnergy, Scrap> MinRate,
        Resource<CoreEnergy> EquilibriumMarketSupply,
        Resource<CoreEnergy> InitialMarketSupply,
        Resource<CoreEnergy> ConsumptionPerWave,
        double SupplyReturnToNormalRate,
        double RateSensitivityToSupply
        );

    private FactionCoreEnergyExchange exchange = null!;

    private Resource<CoreEnergy> supply;

    protected override void Execute()
    {
        supply = Parameters.InitialMarketSupply;
        Owner.TryGetBehaviorIncludingAncestors(out exchange);

        Events.Observe<ResourcesExchanged<CoreEnergy, Scrap>>().Subscribe(e => supply += e.From);
        Events.Observe<WaveStarted>().Subscribe(_ => updateRate());
        Events.Observe<WaveEnded>().Subscribe(onWaveEnd);
    }

    private void onWaveEnd(WaveEnded e)
    {
        Console.WriteLine("Wave ended");
        Console.WriteLine("Current supply: " + supply.Value);

        supply -= Parameters.ConsumptionPerWave;

        var consumption = (supply - Parameters.EquilibriumMarketSupply) * Parameters.SupplyReturnToNormalRate;
        supply -= consumption;

        Console.WriteLine("Supply after consumption: " + supply.Value);

        updateRate();
    }

    private void updateRate()
    {
        var supplyAboveEquilibrium = supply - Parameters.EquilibriumMarketSupply;

        var rate = Parameters.BaseRate + new ExchangeRate<CoreEnergy, Scrap>(supplyAboveEquilibrium.Value * Parameters.RateSensitivityToSupply);

        if (rate < Parameters.MinRate)
        {
            rate = Parameters.MinRate;
        }

        exchange.SetExchangeRate(rate);
    }
}
