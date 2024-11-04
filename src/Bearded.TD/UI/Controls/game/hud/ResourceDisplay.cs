using System;
using System.Reactive.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class ResourceDisplay
{
    private Binding<Resource<Scrap>> currentScrap { get; } = new();
    private Binding<Resource<CoreEnergy>> currentCoreEnergy { get; } = new();

    private Binding<Resource<Scrap>> scrapLeftThisWave { get; } = new();
    private Binding<Resource<CoreEnergy>> coreEnergyLeftThisWave { get; } = new();

    public IReadonlyBinding<Resource<Scrap>> CurrentScrap => currentScrap;
    public IReadonlyBinding<Resource<CoreEnergy>> CurrentCoreEnergy => currentCoreEnergy;

    public IReadonlyBinding<Resource<Scrap>> ScrapLeftThisWave => scrapLeftThisWave;
    public IReadonlyBinding<Resource<CoreEnergy>> CoreEnergyLeftThisWave => coreEnergyLeftThisWave;

    public CoreEnergyExchange Exchange { get; } = new();

    private readonly record struct ObservedResources(
        Resource<Scrap> Scrap,
        Resource<CoreEnergy> CoreEnergy,
        Resource<CoreEnergy> CoreEnergyLeft,
        ExchangeRate<CoreEnergy, Scrap> ExchangeRate,
        double ExchangePercentage
        );

    public void Initialize(GameInstance game)
    {
        Exchange.Initialize(game);

        var faction = game.Me.Faction;
        faction.TryGetBehaviorIncludingAncestors(out FactionResources? resources);
        faction.TryGetBehaviorIncludingAncestors(out FactionCoreDeposit? coreDeposit);
        faction.TryGetBehaviorIncludingAncestors(out FactionCoreEnergyExchange? exchange);

        Observable.CombineLatest(
            observe<Scrap>(game, resources),
            observe<CoreEnergy>(game, resources),
            game.Meta.Events.Observe<AvailableResourcesChanged<CoreEnergy>>()
                .Select(e => e.NewAmount)
                .StartWith(coreDeposit?.AvailableCoreInCurrentWave ?? Resource<CoreEnergy>.Zero),
            game.Meta.Events.Observe<ExchangeRateChanged>()
                .Select(e => e.Rate)
                .StartWith(exchange?.Rate ?? new ExchangeRate<CoreEnergy, Scrap>(1.0)),
            game.Meta.Events.Observe<ExchangePercentageChanged>()
                .Select(e => e.Percentage)
                .StartWith(exchange?.Percentage ?? 1.0),
            (scrap, core, coreLeft, rate, percentage) => new ObservedResources(scrap, core, coreLeft, rate, percentage)
        ).Subscribe(updateBindings);
    }

    private void updateBindings(ObservedResources resources)
    {
        currentScrap.SetFromSource(resources.Scrap);
        currentCoreEnergy.SetFromSource(resources.CoreEnergy);

        var totalCoreEnergy = resources.CoreEnergy + resources.CoreEnergyLeft;

        var coreEnergyConvertedToScrap = resources.CoreEnergyLeft * resources.ExchangePercentage;

        if (coreEnergyConvertedToScrap > totalCoreEnergy)
        {
            coreEnergyConvertedToScrap = totalCoreEnergy;
        }

        var coreEnergyPayout = resources.CoreEnergyLeft - coreEnergyConvertedToScrap;
        var scrapPayout = coreEnergyConvertedToScrap * resources.ExchangeRate;

        scrapLeftThisWave.SetFromSource(scrapPayout);
        coreEnergyLeftThisWave.SetFromSource(coreEnergyPayout);
    }

    private static IObservable<Resource<T>> observe<T>(GameInstance game, FactionResources? resources)
        where T : IResourceType
    {
        return game.Meta.Events.Observe<ResourcesChanged<T>>()
            .Where(e => e.Resources == resources)
            .StartWith(new ResourcesChanged<T>(null!, resources?.GetCurrent<T>() ?? Resource<T>.Zero))
            .Select(e => e.NewAmount);
    }
}
