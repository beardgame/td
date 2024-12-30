using System;
using System.Reactive.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class ResourceDisplay
{
    public IReadonlyBinding<Resource<Scrap>> CurrentScrap { get; private set; } = null!;
    public IReadonlyBinding<Resource<CoreEnergy>> CurrentCoreEnergy { get; private set; } = null!;

    public IReadonlyBinding<Resource<Scrap>> ScrapLeftThisWave { get; private set; } = null!;
    public IReadonlyBinding<Resource<CoreEnergy>> CoreEnergyLeftThisWave { get; private set; } = null!;

    public IObservable<Resource<Scrap>> ScrapGained { get; private set; } = null!;
    public IObservable<Resource<Scrap>> ScrapSpent { get; private set; } = null!;

    public IObservable<Resource<CoreEnergy>> CoreEnergyGained { get; private set; } = null!;
    public IObservable<Resource<CoreEnergy>> CoreEnergySpent { get; private set; } = null!;

    public CoreEnergyExchange Exchange { get; } = new();

    public void Initialize(GameInstance game)
    {
        var exchange = new ResourceExchangeObserver(game);
        ScrapLeftThisWave = exchange.ScrapLeftThisWave.Transform(resourceToInteger);
        CoreEnergyLeftThisWave = exchange.CoreEnergyLeftThisWave.Transform(resourceToInteger);

        Exchange.Initialize(game, exchange);

        var faction = game.Me.Faction;
        faction.TryGetBehaviorIncludingAncestors(out FactionResources? resources);

        (CurrentScrap, ScrapGained, ScrapSpent) = setup<Scrap>(game, resources);
        (CurrentCoreEnergy, CoreEnergyGained, CoreEnergySpent) = setup<CoreEnergy>(game, resources);
    }

    private static (
        IReadonlyBinding<Resource<T>> Total,
        IObservable<Resource<T>> Gained,
        IObservable<Resource<T>> Spent
        ) setup<T>(GameInstance game, FactionResources? resources)
        where T : IResourceType
    {
        var scrap = observe<T>(game, resources);
        var (gained, spent) = gainedAndSpent(scrap);
        var total = scrap.BindDisplayOnly(out _);
        return (total, gained, spent);
    }

    private static (IObservable<Resource<T>> Gained, IObservable<Resource<T>> Spent) gainedAndSpent<T>(
        IObservable<Resource<T>> scrap)
        where T : IResourceType
    {
        return scrap
            .LastTwo()
            .Select(r => r.Current - r.Previous)
            .Split(r => r > Resource<T>.Zero);
    }

    private static IObservable<Resource<T>> observe<T>(GameInstance game, FactionResources? resources)
        where T : IResourceType
    {
        return game.Meta.Events.Observe<ResourcesChanged<T>>()
            .Where(e => e.Resources == resources)
            .StartWith(new ResourcesChanged<T>(null!, resources?.GetCurrent<T>() ?? Resource<T>.Zero))
            .Select(e => resourceToInteger(e.NewAmount))
            .DistinctUntilChanged();
    }

    private static Resource<T> resourceToInteger<T>(Resource<T> resource)
        where T : IResourceType
    {
        return new Resource<T>(Math.Floor(resource.Value));
    }
}
