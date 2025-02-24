using System.Reactive.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class ResourceExchangeObserver
{
    public IReadonlyBinding<Resource<Scrap>> ScrapLeftThisWave { get; }
    public IReadonlyBinding<Resource<CoreEnergy>> CoreEnergyLeftThisWave { get; }
    public IReadonlyBinding<double> MaxExchangePercentage { get; }

    public ResourceExchangeObserver(GameInstance game)
    {
        var faction = game.Me.Faction;
        faction.TryGetBehaviorIncludingAncestors(out FactionCoreDeposit? coreDeposit);
        faction.TryGetBehaviorIncludingAncestors(out FactionCoreEnergyExchange? exchange);

        var changes = game.Meta.Events.Observe<AvailableResourcesChanged<CoreEnergy>>()
            .Where(e => e.Faction == faction)
            .Select(e => e.NewAmount)
            .StartWith(coreDeposit?.AvailableCoreInCurrentWave ?? Resource<CoreEnergy>.Zero)
            .CombineLatest(
                observeChange<ResourcesChanged<CoreEnergy>>(game),
                observeChange<ExchangeRateChanged>(game),
                observeChange<ExchangePercentageChanged>(game),
                (payout, _, _, _) => exchange?.PreviewExchange(payout) ?? default
            );

        CoreEnergyLeftThisWave = changes.BindDisplayOnly(c => c.CoreEnergyChanged, out _);
        ScrapLeftThisWave = changes.BindDisplayOnly(c => c.ScrapChange, out _);

        MaxExchangePercentage = changes
            .Select(c => c.MaximumExchangePercentage)
            .Where(p => p > 0 && double.IsFinite(p))
            .BindDisplayOnly(out _);
    }

    private static System.IObservable<Void> observeChange<T>(GameInstance game)
        where T : struct, IGlobalEvent
    {
        return game.Meta.Events.Observe<T>()
            .Select(_ => default(Void))
            .StartWith(default(Void));
    }
}
